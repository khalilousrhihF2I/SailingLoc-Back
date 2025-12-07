using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs.Dashboard;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;

        public DashboardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AdminStatsDto> GetStatsAsync()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalBoats = await _db.Boats.CountAsync();
            var totalBookings = await _db.Bookings.CountAsync();
            var totalRevenue = await _db.Bookings.Where(b => b.PaymentStatus == "succeeded").SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;
            var pendingVerifications = await _db.Boats.CountAsync(b => !b.IsVerified);
            var pendingDocuments = await _db.UserDocuments.CountAsync(d => !d.IsVerified);
            var disputes = await _db.Messages.CountAsync(m => m.Subject != null && m.Subject.Contains("litige"));

            return new AdminStatsDto
            {
                TotalUsers = totalUsers,
                TotalBoats = totalBoats,
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                PendingVerifications = pendingVerifications,
                PendingDocuments = pendingDocuments,
                Disputes = disputes
            };
        }

        public async Task<List<AdminUserDto>> GetUsersAsync()
        {
            var q = await _db.Users
                .Include(u => u.BoatsOwned).ThenInclude(b => b.Bookings)
                .Include(u => u.UserDocuments)
                .AsNoTracking()
                .ToListAsync();

            return q.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Name = u.FirstName + " " + u.LastName,
                Email = u.Email,
                Type = u.UserType,
                Verified = u.Verified,
                BoatsCount = u.BoatsOwned?.Count ?? 0,
                TotalRevenue = (decimal)(u.BoatsOwned?.Sum(b => b.Bookings?.Sum(bb => (decimal?)bb.TotalPrice) ?? 0) ?? 0),
                MemberSince = u.MemberSince.ToString("yyyy-MM-dd")
            }).ToList();
        }

        public async Task<List<AdminBoatDto>> GetBoatsAsync()
        {
            var q = await _db.Boats.Include(b => b.Owner).AsNoTracking().ToListAsync();
            return q.Select(b => new AdminBoatDto
            {
                Id = b.Id,
                Name = b.Name,
                OwnerId = b.OwnerId,
                OwnerName = b.Owner != null ? b.Owner.FirstName + " " + b.Owner.LastName : string.Empty,
                Price = b.Price,
                IsVerified = b.IsVerified,
                Type = b.Type,
                Image = b.Image,                
                ReviewCount = b.ReviewCount
            }).ToList();
        }

        public async Task<List<AdminBookingDto>> GetBookingsAsync()
        {
            var q = await _db.Bookings.Include(b => b.Boat).Include(b => b.Renter).AsNoTracking().ToListAsync();
            return q.Select(b => new AdminBookingDto
            {
                Id = b.Id,
                BoatId = b.BoatId,
                BoatName = b.Boat?.Name ?? string.Empty,
                RenterId = b.RenterId,
                RenterName = b.Renter != null ? b.Renter.FirstName + " " + b.Renter.LastName : string.Empty,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminActivityDto>> GetActivityAsync()
        {
            var activities = new List<AdminActivityDto>();

            var lastBooking = await _db.Bookings.OrderByDescending(b => b.CreatedAt).FirstOrDefaultAsync();
            if (lastBooking != null)
            {
                activities.Add(new AdminActivityDto
                {
                    Type = "booking",
                    Description = $"Booking {lastBooking.Id} by {lastBooking.RenterName}",
                    OccurredAt = lastBooking.CreatedAt
                });
            }

            var lastBoat = await _db.Boats.OrderByDescending(b => b.CreatedAt).FirstOrDefaultAsync();
            if (lastBoat != null)
            {
                activities.Add(new AdminActivityDto
                {
                    Type = "boat",
                    Description = $"Boat {lastBoat.Name} created",
                    OccurredAt = lastBoat.CreatedAt
                });
            }

            var lastUser = await _db.Users.OrderByDescending(u => u.CreatedAt).FirstOrDefaultAsync();
            if (lastUser != null)
            {
                activities.Add(new AdminActivityDto
                {
                    Type = "user",
                    Description = $"User {lastUser.FirstName} {lastUser.LastName} registered",
                    OccurredAt = lastUser.CreatedAt
                });
            }

            return activities;
        }

        public async Task<AdminPaymentStatsDto> GetPaymentStatsAsync()
        {
            var paid = await _db.Bookings.Where(b => b.PaymentStatus == "succeeded").SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;
            var pending = await _db.Bookings.Where(b => b.PaymentStatus == "pending").SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;
            var platformFee = Math.Round(paid * 0.10m, 2);

            return new AdminPaymentStatsDto
            {
                Paid = paid,
                Pending = pending,
                PlatformFee = platformFee
            };
        }
    }
}
