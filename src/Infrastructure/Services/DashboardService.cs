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
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminActivityDto>> GetActivityAsync()
        {
            // Pull real activity from AuditLogs + recent entities
            var activities = new List<AdminActivityDto>();

            // Recent audit log entries (last 20)
            var recentLogs = await _db.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(20)
                .ToListAsync();

            foreach (var log in recentLogs)
            {
                activities.Add(new AdminActivityDto
                {
                    Type = log.Action?.ToLower().Contains("login") == true ? "login"
                         : log.Action?.ToLower().Contains("register") == true ? "user"
                         : log.Action?.ToLower().Contains("booking") == true ? "booking"
                         : log.Action?.ToLower().Contains("boat") == true ? "boat"
                         : log.Action?.ToLower().Contains("review") == true ? "review"
                         : log.Action?.ToLower().Contains("dispute") == true ? "dispute"
                         : "system",
                    Description = log.Details ?? log.Action,
                    OccurredAt = log.Timestamp,
                    Action = log.Action,
                    UserId = log.UserId?.ToString(),
                    Ip = log.Ip
                });
            }

            // If no audit logs yet, fall back to recent entity creation
            if (activities.Count == 0)
            {
                var recentBookings = await _db.Bookings
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                foreach (var bk in recentBookings)
                {
                    activities.Add(new AdminActivityDto
                    {
                        Type = "booking",
                        Description = $"Réservation #{bk.Id} par {bk.RenterName}",
                        OccurredAt = bk.CreatedAt,
                        Action = "BOOKING_CREATE"
                    });
                }

                var recentBoats = await _db.Boats
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                foreach (var bt in recentBoats)
                {
                    activities.Add(new AdminActivityDto
                    {
                        Type = "boat",
                        Description = $"Bateau \"{bt.Name}\" ajouté",
                        OccurredAt = bt.CreatedAt,
                        Action = "BOAT_CREATE"
                    });
                }

                var recentUsers = await _db.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                foreach (var u in recentUsers)
                {
                    activities.Add(new AdminActivityDto
                    {
                        Type = "user",
                        Description = $"{u.FirstName} {u.LastName} inscrit(e)",
                        OccurredAt = u.CreatedAt,
                        Action = "USER_REGISTER"
                    });
                }

                activities = activities.OrderByDescending(a => a.OccurredAt).Take(20).ToList();
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
