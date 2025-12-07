using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs.RenterDashboard;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class RenterDashboardService : IRenterDashboardService
    {
        private readonly ApplicationDbContext _db;

        public RenterDashboardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RenterStatsDto> GetStatsAsync(Guid userId)
        {
            var pending = await _db.Bookings.CountAsync(b => b.RenterId == userId && b.Status == "pending");
            var confirmed = await _db.Bookings.CountAsync(b => b.RenterId == userId && b.Status == "confirmed");
            var completed = await _db.Bookings.CountAsync(b => b.RenterId == userId && b.Status == "completed");
            var cancelled = await _db.Bookings.CountAsync(b => b.RenterId == userId && b.Status == "cancelled");

            return new RenterStatsDto { Pending = pending, Confirmed = confirmed, Completed = completed, Cancelled = cancelled };
        }

        public async Task<List<RenterBookingDto>> GetBookingsAsync(Guid userId)
        {
            var list = await _db.Bookings
                .Where(b => b.RenterId == userId)
                .Include(b => b.Boat).ThenInclude(x => x.Images)
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return list.Select(b => new RenterBookingDto
            {
                Id = b.Id,
                BoatId = b.BoatId,
                BoatName = b.Boat?.Name ?? string.Empty,
                BoatImage = b.Boat?.Image ?? b.Boat?.Images?.FirstOrDefault()?.ImageUrl,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            }).ToList();
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

        public async Task<List<RenterDocumentDto>> GetDocumentsAsync(Guid userId)
        {
            var docs = await _db.UserDocuments.Where(d => d.UserId == userId).AsNoTracking().ToListAsync();
            return docs.Select(d => new RenterDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                DocumentUrl = d.DocumentUrl,
                Verified = d.IsVerified,
                UploadedAt = d.UploadedAt
            }).ToList();
        }

        public Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(Guid userId)
        {
            // If you have a payment methods table, return real data. For now return mock list.
            var list = new List<PaymentMethodDto>
            {
                new PaymentMethodDto { Id = "pm_1", Brand = "Visa", Last4 = "4242", Expiry = "12/2025" }
            };
            return Task.FromResult(list);
        }
    }
}
