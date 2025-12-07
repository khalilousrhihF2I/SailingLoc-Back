using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly ApplicationDbContext _db;
        private readonly DestinationRepository _repo;

        public DestinationService(ApplicationDbContext db, DestinationRepository repo)
        {
            _db = db;
            _repo = repo;
        }

        public async Task<IEnumerable<Destination>> GetAllAsync()
        {
            return await _db.Destinations.AsNoTracking().ToListAsync();
        }

        public Task<Destination?> GetByIdAsync(int id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Destination>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<Destination>();
            var pattern = $"%{query}%";
            return await _db.Destinations
                .Where(d => EF.Functions.Like(d.Name, pattern) || EF.Functions.Like(d.Country, pattern) || EF.Functions.Like(d.Region, pattern))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetByRegionAsync(string region)
        {
            if (string.IsNullOrWhiteSpace(region)) return Array.Empty<Destination>();
            var pattern = $"%{region}%";
            return await _db.Destinations
                .Where(d => EF.Functions.Like(d.Region, pattern))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Destination>> GetPopularAsync(int limit = 4)
        {
            // Popular: order by number of boats if relation exists, otherwise by some popularity metric
            // Try to use Boats count if navigation exists
            try
            {
                return await _db.Destinations
                    .Select(d => new { Dest = d, BoatsCount = d.Boats.Count })
                    .OrderByDescending(x => x.BoatsCount)
                    .Take(limit)
                    .Select(x => x.Dest)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch
            {
                return await _db.Destinations
                    .OrderByDescending(d => d.Id)
                    .Take(limit)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<Destination> CreateAsync(Destination dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            dto.CreatedAt = DateTime.UtcNow;
            await _db.Destinations.AddAsync(dto);
            await _db.SaveChangesAsync();
            return dto;
        }

        public async Task<Destination> UpdateAsync(int id, Destination dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var existing = await _db.Destinations.FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null) throw new KeyNotFoundException("Destination not found");
            existing.Name = dto.Name;
            existing.Region = dto.Region;
            existing.Country = dto.Country;
            existing.Description = dto.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.Destinations.FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null) return false;
            _db.Destinations.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
