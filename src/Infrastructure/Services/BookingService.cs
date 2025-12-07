using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Notifications;
using Core.Models.Templates;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly BookingRepository _repo;
        private readonly IEmailService _emailService;

        public BookingService(ApplicationDbContext db, BookingRepository repo, IEmailService emailService)
        {
            _db = db;
            _repo = repo;
            _emailService = emailService;
        }

        private string GenerateBookingId()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var rand = Guid.NewGuid().ToString().Split('-')[0];
            return $"BK{date}-{rand}";
        }

        private BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                Id = b.Id,
                BoatId = b.BoatId,
                BoatName = b.Boat?.Name ?? string.Empty,
                BoatImage = b.Boat?.Image ?? b.Boat?.Images?.FirstOrDefault()?.ImageUrl,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalPrice = b.TotalPrice,
                ServiceFee = b.ServiceFee,
                Status = b.Status,
                OwnerId = b.Boat?.OwnerId ?? Guid.Empty,
                OwnerName = b.Boat?.Owner != null ? $"{b.Boat.Owner.FirstName} {b.Boat.Owner.LastName}" : string.Empty,
                OwnerEmail = b.Boat?.Owner?.Email ?? string.Empty,
                OwnerPhoneNumber = b.Boat?.Owner?.PhoneNumber ?? string.Empty,
                RenterId = b.RenterId,
                RenterName = b.RenterName,
                RenterEmail = b.RenterEmail,
                RenterPhoneNumber = b.RenterPhone ?? string.Empty,
                CreatedAt = b.CreatedAt
            };
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsAsync(BookingFilters filters)
        {
            var q = _db.Bookings
                .Include(b => b.Boat).ThenInclude(x => x.Images)
                .Include(b => b.Boat).ThenInclude(x => x.Owner)
                .AsNoTracking()
                .AsQueryable();

            if (filters?.RenterId != null) q = q.Where(b => b.RenterId == filters.RenterId.Value);
            if (filters?.OwnerId != null) q = q.Where(b => b.Boat.OwnerId == filters.OwnerId.Value);
            if (!string.IsNullOrWhiteSpace(filters?.Status)) q = q.Where(b => b.Status == filters.Status);
            if (filters?.StartDate != null) q = q.Where(b => b.EndDate >= filters.StartDate.Value);
            if (filters?.EndDate != null) q = q.Where(b => b.StartDate <= filters.EndDate.Value);

            var list = await q.ToListAsync();
            return list.Select(MapToDto);
        }

        public async Task<BookingDto?> GetBookingByIdAsync(string id)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null) return null;
            return MapToDto(b);
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
        {
            // validate boat exists
            var boat = await _db.Boats.Include(b => b.Images).Include(b => b.Owner).FirstOrDefaultAsync(b => b.Id == dto.BoatId);
            if (boat == null) throw new KeyNotFoundException("Bateau introuvable");

            // check availability: overlap with blocked periods or bookings
            var overlappingAvailability = await _db.BoatAvailabilities
                .Where(a => a.BoatId == dto.BoatId && !a.IsAvailable && a.StartDate < dto.EndDate && a.EndDate > dto.StartDate)
                .AsNoTracking()
                .AnyAsync();
            if (overlappingAvailability) throw new InvalidOperationException("Le bateau n'est pas disponible pour la période sélectionnée");

            var overlappingBookings = await _db.Bookings
                .Where(b => b.BoatId == dto.BoatId && b.Status != "cancelled" && b.StartDate < dto.EndDate && b.EndDate > dto.StartDate)
                .AsNoTracking()
                .AnyAsync();
            if (overlappingBookings) throw new InvalidOperationException("Le bateau est déjà réservé pour la période sélectionnée");

            // prevent renter from having overlapping bookings on other boats
            var overlappingRenterBookings = await _db.Bookings
                .Where(b => b.RenterId == dto.RenterId && b.Status != "cancelled" && b.StartDate < dto.EndDate && b.EndDate > dto.StartDate)
                .AsNoTracking()
                .AnyAsync();
            if (overlappingRenterBookings) throw new InvalidOperationException("Vous avez déjà une réservation qui chevauche la période sélectionnée");

            // create booking
            var booking = new Booking
            {
                Id = GenerateBookingId(),
                BoatId = dto.BoatId,
                RenterId = dto.RenterId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DailyPrice = dto.DailyPrice,
                ServiceFee = dto.ServiceFee,
                Subtotal = dto.DailyPrice * (decimal)(dto.EndDate - dto.StartDate).TotalDays,
                TotalPrice = dto.DailyPrice * (decimal)(dto.EndDate - dto.StartDate).TotalDays + dto.ServiceFee,
                Status = "pending",
                PaidAt = DateTime.UtcNow,
                PaymentIntentId = $"pi_{Guid.NewGuid().ToString("N")}",
                PaymentStatus = "succeeded",
                RenterName = dto.RenterName,
                RenterEmail = dto.RenterEmail,
                RenterPhone = dto.RenterPhone,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(booking);
            await _repo.SaveChangesAsync();

            // add availability record referencing this booking
            var avail = new BoatAvailability
            {
                BoatId = booking.BoatId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                IsAvailable = false,
                ReferenceType = "booking",
                Reason = "Réservation Client",
                ReferenceId = booking.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _db.BoatAvailabilities.AddAsync(avail);
            await _db.SaveChangesAsync();

            // reload with includes
            var created = await _repo.GetByIdAsync(booking.Id);

            // send notification emails (owner + renter)
            try
            {
                var model = new ReservationTemplateModel
                {
                    BrandName = "SailingLoc",
                    BookingId = booking.Id,
                    BoatId = booking.BoatId,
                    BoatName = boat.Name,
                    RenterName = booking.RenterName,
                    RenterId = booking.RenterId,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    TotalPrice = booking.TotalPrice
                };

                var recipients = new List<string>();
                if (!string.IsNullOrWhiteSpace(booking.RenterEmail)) recipients.Add(booking.RenterEmail);
                if (boat.Owner != null && !string.IsNullOrWhiteSpace(boat.Owner.Email)) recipients.Add(boat.Owner.Email);

                if (recipients.Any())
                {
                    // fire-and-forget but await to handle failures
                    await _emailService.SendReservationCreatedEmailAsync(recipients, model);
                }
            }
            catch (Exception ex)
            {
                // don't fail the booking if email fails; log to console for now
                Console.WriteLine($"Erreur lors de l'envoi des notifications de réservation: {ex.Message}");
            }

            return MapToDto(created!);
        }

        public async Task<BookingDto> UpdateBookingAsync(string id, UpdateBookingDto dto)
        {
            var existing = await _db.Bookings
                .Include(b => b.Boat).ThenInclude(x => x.Owner)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) throw new KeyNotFoundException("Réservation introuvable");
            // only status allowed
            existing.Status = dto.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            _db.Bookings.Update(existing);
            await _db.SaveChangesAsync();

            // if booking confirmed, notify renter and admins
            try
            {
                if (string.Equals(dto.Status, "confirmed", StringComparison.OrdinalIgnoreCase))
                {
                    var model = new ReservationApprovedTemplateModel
                    {
                        BrandName = "SailingLoc",
                        BookingId = existing.Id,
                        BoatId = existing.BoatId,
                        BoatName = existing.Boat?.Name ?? string.Empty,
                        RenterName = existing.RenterName,
                        StartDate = existing.StartDate,
                        EndDate = existing.EndDate
                    };

                    var recipients = new List<string>();
                    //if (!string.IsNullOrWhiteSpace(existing.RenterEmail)) recipients.Add(existing.RenterEmail);

                    // include admins
                    var adminEmails = await _db.Users
                        .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                        .Join(_db.Roles, j => j.ur.RoleId, r => r.Id, (j, r) => new { j.u, Role = r })
                        .Where(x => x.Role.Name == "Admin")
                        .Select(x => x.u.Email)
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .ToListAsync();

                    recipients.AddRange(adminEmails);

                    if (recipients.Any())
                    {
                        await _emailService.SendReservationApprovedEmailAsync(recipients, model);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de la notification de confirmation: {ex.Message}");
            }

            return MapToDto(await _repo.GetByIdAsync(id) ?? existing);
        }

        public async Task<bool> CancelBookingAsync(string id)
        {
            var existing = await _db.Bookings
                .Include(b => b.Boat).ThenInclude(x => x.Owner)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return false;
            existing.Status = "cancelled";
            existing.CancelledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // remove related availability that references this booking
            var avail = await _db.BoatAvailabilities.FirstOrDefaultAsync(a => a.ReferenceType == "booking" && a.ReferenceId == id);
            if (avail != null)
            {
                _db.BoatAvailabilities.Remove(avail);
                await _db.SaveChangesAsync();
            }

            // send cancellation notification to owner, renter and admins
            try
            {
                var model = new CancellationRequestTemplateModel
                {
                    BrandName = "SailingLoc",
                    BookingId = existing.Id,
                    BoatId = existing.BoatId,
                    BoatName = existing.Boat?.Name ?? string.Empty,
                    RequesterName = existing.RenterName,
                    RequesterId = existing.RenterId,
                    Reason = string.Empty,
                    RequestedAt = DateTime.UtcNow
                };

                var recipients = new List<string>();
                //if (!string.IsNullOrWhiteSpace(existing.RenterEmail)) recipients.Add(existing.RenterEmail);
                //if (existing.Boat?.Owner != null && !string.IsNullOrWhiteSpace(existing.Boat.Owner.Email)) recipients.Add(existing.Boat.Owner.Email);

                // admins
                var adminEmails = await _db.Users
                    .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                    .Join(_db.Roles, j => j.ur.RoleId, r => r.Id, (j, r) => new { j.u, Role = r })
                    .Where(x => x.Role.Name == "Admin")
                    .Select(x => x.u.Email)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToListAsync();

                recipients.AddRange(adminEmails);

                if (recipients.Any())
                {
                    await _emailService.SendCancellationRequestEmailAsync(recipients, model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi des notifications d'annulation: {ex.Message}");
            }

            return true;
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByRenterAsync(Guid renterId)
        {
            var list = await _db.Bookings
                .Where(b => b.RenterId == renterId)
                .Include(b => b.Boat).ThenInclude(x => x.Images)
                .Include(b => b.Boat).ThenInclude(x => x.Owner)
                .AsNoTracking()
                .ToListAsync();
            return list.Select(MapToDto);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByOwnerAsync(Guid ownerId)
        {
            var list = await _db.Bookings
                .Include(b => b.Boat).ThenInclude(x => x.Images)
                .Include(b => b.Boat).ThenInclude(x => x.Owner)
                .Where(b => b.Boat.OwnerId == ownerId)
                .AsNoTracking()
                .ToListAsync();
            return list.Select(MapToDto);
        }
    }
}
