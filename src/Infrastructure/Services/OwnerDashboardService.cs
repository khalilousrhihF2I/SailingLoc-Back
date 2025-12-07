using Core.DTOs.OwnerDashboard;
using Core.DTOs.RenterDashboard;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OwnerDashboardService : IOwnerDashboardService
    {
        private readonly ApplicationDbContext _db;

        public OwnerDashboardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<OwnerStatsDto> GetStatsAsync(Guid ownerId)
        {
            var boatsCount = await _db.Boats.CountAsync(b => b.OwnerId == ownerId);
            var bookingsCount = await _db.Bookings.CountAsync(b => b.Boat.OwnerId == ownerId);
            var totalRevenue = await _db.Bookings.Where(b => b.Boat.OwnerId == ownerId && b.Status == "completed").SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;
            var pendingBookings = await _db.Bookings.CountAsync(b => b.Boat.OwnerId == ownerId && b.Status == "pending");

            // Occupancy: simple estimation over last 30 days
            var rangeStart = DateTime.UtcNow.AddDays(-30);
            var rangeEnd = DateTime.UtcNow;
            var totalDays = (rangeEnd - rangeStart).TotalDays;

            double occupancy = 0.0;

            if (boatsCount > 0 && totalDays > 0)
            {
                var bookedDaysList = await _db.Bookings
                    .Where(b => b.Boat.OwnerId == ownerId && b.Status != "cancelled" && b.EndDate >= rangeStart && b.StartDate <= rangeEnd)
                    .Select(b => new { OverlapStart = b.StartDate < rangeStart ? rangeStart : b.StartDate, OverlapEnd = b.EndDate > rangeEnd ? rangeEnd : b.EndDate })
                    .AsNoTracking()
                    .ToListAsync();

                var daysBooked = bookedDaysList.Sum(x => (x.OverlapEnd - x.OverlapStart).TotalDays);
                // total possible days = boatsCount * totalDays
                var possibleDays = boatsCount * totalDays;
                occupancy = possibleDays > 0 ? (daysBooked / possibleDays) * 100.0 : 0.0;
            }

            return new OwnerStatsDto
            {
                BoatsCount = boatsCount,
                BookingsCount = bookingsCount,
                TotalRevenue = totalRevenue,
                PendingBookings = pendingBookings,
                OccupancyRate = Math.Round(occupancy, 2)
            };
        }

        

        public async Task<List<OwnerBoatDto>> GetBoatsAsync(Guid ownerId)
        {
            var boats = await _db.Boats.Include(b => b.Images).Where(b => b.OwnerId == ownerId && !b.IsDeleted).AsNoTracking().ToListAsync();
            return boats.Select(b => new OwnerBoatDto
            {
                Id = b.Id,
                Name = b.Name,
                Price = b.Price,
                IsActive = b.IsActive,
                Type = b.Type,
                Image = b.Image,
                ReviewCount = b.ReviewCount,
                IsVerified = b.IsVerified,
                IsDeleted = b.IsDeleted,
                Images = b.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>()
            }).ToList();
        }

        public async Task<List<OwnerBookingDto>> GetBookingsAsync(Guid ownerId)
        {
            var bookings = await _db.Bookings
                .Include(b => b.Boat)
                .Include(b => b.Renter)
                .Where(b => b.Boat.OwnerId == ownerId)
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(b =>
            {
                // determine boat price per day (prefer booking value if available)
                var boatPricePerDay = b.DailyPrice != 0 ? b.DailyPrice : (b.Boat?.Price ?? 0m);

                // owner revenue after 10% commission
                var ownerRevenue = Math.Round(b.TotalPrice * 0.90m, 2);

                // payment status default to "pending" if not set
                var paymentStatus = string.IsNullOrWhiteSpace(b.PaymentStatus) ? "pending" : b.PaymentStatus;

                // renter contact: prefer booking-stored values, fallback to user navigation
                var renterPhone = b.RenterPhone ?? b.Renter?.PhoneNumber;
                var renterEmail = b.RenterEmail ?? b.Renter?.Email;

                return new OwnerBookingDto
                {
                    Id = b.Id,
                    BoatId = b.BoatId,
                    BoatName = b.Boat?.Name ?? string.Empty,
                    BoatType = b.Boat?.Type ?? string.Empty,
                    BoatImage = b.Boat?.Image ?? null,
                    RenterId = b.RenterId,
                    RenterName = b.Renter != null ? b.Renter.FirstName + " " + b.Renter.LastName : (b.RenterName ?? string.Empty),
                    RenterPhone = renterPhone,
                    RenterEmail = renterEmail,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalPrice = b.TotalPrice,
                    BoatPricePerDay = boatPricePerDay,
                    OwnerRevenue = ownerRevenue,
                    PaymentStatus = paymentStatus,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                };
            }).ToList();
        }

        public async Task<OwnerRevenueDto> GetRevenueAsync(Guid ownerId)
        {
            var totalRevenue = await _db.Bookings.Where(b => b.Boat.OwnerId == ownerId && b.Status == "completed").SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;
            var monthAgo = DateTime.UtcNow.AddDays(-30);
            var monthRevenue = await _db.Bookings.Where(b => b.Boat.OwnerId == ownerId && b.Status == "completed" && b.CreatedAt >= monthAgo).SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;
            var upcomingPayments = await _db.Bookings.Where(b => b.Boat.OwnerId == ownerId && b.PaymentStatus == "pending").SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;

            return new OwnerRevenueDto
            {
                TotalRevenue = totalRevenue,
                MonthRevenue = monthRevenue,
                UpcomingPayments = upcomingPayments
            };
        }

        public async Task<OwnerAvailabilityDto> GetAvailabilityAsync(int boatId, Guid ownerId)
        {
            var boat = await _db.Boats.FirstOrDefaultAsync(b => b.Id == boatId && b.OwnerId == ownerId);
            if (boat == null) throw new KeyNotFoundException("Boat not found");

            var periods = new List<OwnerAvailabilityPeriod>();

            var avail = await _db.BoatAvailabilities.Where(a => a.BoatId == boatId).AsNoTracking().ToListAsync();
            periods.AddRange(avail.Select(a => new OwnerAvailabilityPeriod
            {
                Type = "blocked",
                ReferenceId = a.ReferenceId ?? string.Empty,
                StartDate = a.StartDate.ToUniversalTime().ToString("o"),
                EndDate = a.EndDate.ToUniversalTime().ToString("o"),
                Reason = a.Reason
            }));

            var bookings = await _db.Bookings.Where(b => b.BoatId == boatId && b.Status != "cancelled").AsNoTracking().ToListAsync();
            periods.AddRange(bookings.Select(b => new OwnerAvailabilityPeriod
            {
                Type = "booking",
                ReferenceId = b.Id,
                StartDate = b.StartDate.ToUniversalTime().ToString("o"),
                EndDate = b.EndDate.ToUniversalTime().ToString("o"),
                Reason = null
            }));

            return new OwnerAvailabilityDto { Periods = periods.OrderBy(p => p.StartDate).ToList() };
        }

        public async Task<bool> UpdateAvailabilityAsync(int boatId, Guid ownerId, UpdateAvailabilityDto dto)
        {
            var boat = await _db.Boats.FirstOrDefaultAsync(b => b.Id == boatId && b.OwnerId == ownerId);
            if (boat == null) throw new KeyNotFoundException("Boat not found");

            if (dto.Action == "block")
            {
                var a = new Core.Entities.BoatAvailability
                {
                    BoatId = boatId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsAvailable = false,
                    Reason = dto.Reason,
                    ReferenceType = "blocked",
                    ReferenceId = dto.ReferenceId
                };
                await _db.BoatAvailabilities.AddAsync(a);
                await _db.SaveChangesAsync();
                return true;
            }
            else if (dto.Action == "unblock")
            {
                if (string.IsNullOrWhiteSpace(dto.ReferenceId))
                {
                    // remove by exact period
                    var existing = await _db.BoatAvailabilities.FirstOrDefaultAsync(x => x.BoatId == boatId && x.StartDate == dto.StartDate && x.EndDate == dto.EndDate);
                    if (existing == null) return false;
                    _db.BoatAvailabilities.Remove(existing);
                    await _db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    var existing = await _db.BoatAvailabilities.FirstOrDefaultAsync(x => x.BoatId == boatId && x.ReferenceId == dto.ReferenceId);
                    if (existing == null) return false;
                    _db.BoatAvailabilities.Remove(existing);
                    await _db.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task<RenterProfileDto> GetProfileAsync(Guid userId)
        {
            var u = await _db.Users.Include(x => x.Address).FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) throw new KeyNotFoundException("User not found");

            return new RenterProfileDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.PhoneNumber,
                MemberSince = u.MemberSince,
                Avatar = u.AvatarUrl,
                Street = u.Address?.Street,
                City = u.Address?.City,
                State = u.Address?.State,
                PostalCode = u.Address?.PostalCode,
                Country = u.Address?.Country
            };
        }

        public async Task<RenterProfileDto> UpdateProfileAsync(Guid userId, RenterProfileDto updated)
        {
            var u = await _db.Users.Include(x => x.Address).FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) throw new KeyNotFoundException("User not found");

            u.FirstName = updated.FirstName ?? u.FirstName;
            u.LastName = updated.LastName ?? u.LastName;
            u.PhoneNumber = updated.Phone ?? u.PhoneNumber;
            u.AvatarUrl = updated.Avatar ?? u.AvatarUrl;

            if (u.Address == null) u.Address = new Core.Entities.Address();
            u.Address.Street = updated.Street ?? u.Address.Street;
            u.Address.City = updated.City ?? u.Address.City;
            u.Address.State = updated.State ?? u.Address.State;
            u.Address.PostalCode = updated.PostalCode ?? u.Address.PostalCode;
            u.Address.Country = updated.Country ?? u.Address.Country;

            await _db.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }
    }
}
