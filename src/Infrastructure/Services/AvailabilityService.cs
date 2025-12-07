using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ApplicationDbContext _db;
        private readonly AvailabilityRepository _repo;

        public AvailabilityService(ApplicationDbContext db, AvailabilityRepository repo)
        {
            _db = db;
            _repo = repo;
        }

        private static bool Overlaps(DateTime s1, DateTime e1, DateTime s2, DateTime e2)
        {
            return s1 < e2 && s2 < e1;
        }

        public async Task<AvailabilityCheck> CheckAvailabilityAsync(int boatId, DateTime start, DateTime end, string? excludeBookingId)
        {
            if (start >= end) return new AvailabilityCheck { IsAvailable = false, Message = "Invalid date range" };

            // check blocked periods
            var blocked = await _db.BoatAvailabilities
                .Where(a => a.BoatId == boatId && !a.IsAvailable)
                .AsNoTracking()
                .ToListAsync();

            if (blocked.Any(b => Overlaps(b.StartDate, b.EndDate, start, end)))
                return new AvailabilityCheck { IsAvailable = false, Message = "Boat is blocked for this period" };

            // check bookings
            var bookings = await _db.Bookings
                .Where(b => b.BoatId == boatId && b.Status != "cancelled")
                .AsNoTracking()
                .ToListAsync();

            foreach (var bk in bookings)
            {
                if (!string.IsNullOrWhiteSpace(excludeBookingId) && bk.Id.ToString() == excludeBookingId) continue;
                if (Overlaps(bk.StartDate, bk.EndDate, start, end))
                    return new AvailabilityCheck { IsAvailable = false, Message = "Boat is booked during this period" };
            }

            // maintenance or other checks could be added here

            return new AvailabilityCheck { IsAvailable = true, Message = "Available" };
        }

        public async Task<IEnumerable<UnavailablePeriod>> GetUnavailableDatesAsync(int boatId, DateTime? start, DateTime? end)
        {
            var q = _db.BoatAvailabilities.Where(a => a.BoatId == boatId).AsNoTracking().AsQueryable();
            if (start.HasValue) q = q.Where(a => a.EndDate >= start.Value);
            if (end.HasValue) q = q.Where(a => a.StartDate <= end.Value);

            var list = await q.ToListAsync();
            var result = list.Select(a => new UnavailablePeriod
            {
                Type = a.ReferenceType ?? (a.IsAvailable ? "available" : "blocked"),
                ReferenceId = a.ReferenceId,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Reason = a.Reason,
                Details = a.Details
            }).ToList();

            // also include bookings
            var bookings = await _db.Bookings
                .Where(b => b.BoatId == boatId && b.Status != "cancelled")
                .AsNoTracking()
                .ToListAsync();

            result.AddRange(bookings.Select(b => new UnavailablePeriod
            {
                Type = "booking",
                ReferenceId = b.Id.ToString(),
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Reason = null,
                Details = null
            }));

            return result.OrderBy(r => r.StartDate);
        }

        public async Task<UnavailablePeriod> AddUnavailablePeriodAsync(int boatId, AddUnavailablePeriodDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.StartDate >= dto.EndDate) throw new ArgumentException("Invalid date range");

            var a = new BoatAvailability
            {
                BoatId = boatId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsAvailable = false,
                ReferenceType = dto.Type,
                Reason = dto.Reason
            };

            await _repo.AddAsync(a);
            await _repo.SaveChangesAsync();

            return new UnavailablePeriod
            {
                Type = a.ReferenceType,
                ReferenceId = a.ReferenceId,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Reason = a.Reason
            };
        }

        public async Task<bool> RemoveUnavailablePeriodAsync(int boatId, DateTime start)
        {
            var existing = await _db.BoatAvailabilities.FirstOrDefaultAsync(a => a.BoatId == boatId && a.StartDate == start);
            if (existing == null) return false;
            _repo.Remove(existing);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BlockPeriodAsync(CreateAvailabilityDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.StartDate >= dto.EndDate) return false;

            var a = new BoatAvailability
            {
                BoatId = dto.BoatId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsAvailable = dto.IsAvailable,
                Reason = dto.Reason,
                ReferenceType = dto.IsAvailable ? "available" : "blocked"
            };

            await _repo.AddAsync(a);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnblockPeriodAsync(int availabilityId)
        {
            var existing = await _repo.GetByIdAsync(availabilityId);
            if (existing == null) return false;
            _repo.Remove(existing);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}
