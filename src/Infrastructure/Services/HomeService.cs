using Core.DTOs.Home;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _db;

        public HomeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<HomeDto> GetHomeDataAsync()
        {
            var dto = new HomeDto();

            // top boat types
            var topTypes = await _db.Boats
                .AsNoTracking()
                .Where(b => b.IsActive && b.IsVerified)
                .GroupBy(b => b.Type)
                .Select(g => new TopBoatTypeDto { Type = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToListAsync();

            dto.TopBoatTypes = topTypes;

            // popular boats: top 6 by rating desc, then by review count desc (ViewCount not present in model)
            var popular = await _db.Boats
                .AsNoTracking()
                .Where(b => b.IsActive && b.IsVerified)
                .OrderByDescending(b => b.Rating)
                .ThenByDescending(b => b.ReviewCount)
                .Take(6)
                .Select(b => new PopularBoatDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Type = b.Type,
                    Location = b.Location,
                    City = b.City,
                    DestinationId = b.DestinationId,
                    Country = b.Country,
                    Price = b.Price,
                    Capacity = b.Capacity,
                    Cabins = b.Cabins,
                    Length = b.Length,
                    Year = b.Year,
                    Image = b.Image,
                    Rating = b.Rating,
                    ReviewCount = b.ReviewCount,
                    Equipment = b.Equipment,
                    Description = b.Description,
                    OwnerId = b.OwnerId,
                    IsActive = b.IsActive,
                    IsVerified = b.IsVerified
                })
                .ToListAsync();

            // Load owner names & avatars in memory - join with users
            var ownerIds = popular.Select(p => p.OwnerId).Distinct().ToList();
            var owners = await _db.Users
                .AsNoTracking()
                .Where(u => ownerIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim(), Avatar = u.AvatarUrl })
                .ToListAsync();

            foreach (var p in popular)
            {
                var owner = owners.FirstOrDefault(o => o.Id == p.OwnerId);
                if (owner != null)
                {
                    p.OwnerName = owner.Name;
                    p.OwnerAvatar = owner.Avatar;
                }
            }

            dto.PopularBoats = popular;

            return dto;
        }
    }
}
