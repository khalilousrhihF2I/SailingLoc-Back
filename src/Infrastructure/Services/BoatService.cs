using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class BoatService : IBoatService
    {
        private readonly BoatRepository _repo;

        public BoatService(BoatRepository repo)
        {
            _repo = repo;
        }

        private static BoatDto MapToDto(Boat b)
        {
            return new BoatDto
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
                Owner = new BoatOwnerDto
                {
                    Id = b.OwnerId,
                    Name = b.Owner is not null ? ($"{b.Owner.FirstName} {b.Owner.LastName}".Trim()) : null,
                    AvatarUrl = b.Owner?.AvatarUrl
                },
                IsActive = b.IsActive,
                IsVerified = b.IsVerified,
                IsDeleted = b.IsDeleted,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                Images = b.Images?.Select(i => new BoatImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption,
                    DisplayOrder = i.DisplayOrder
                }).OrderBy(i => i.DisplayOrder).ToList() ?? new List<BoatImageDto>(),
                Availabilities = b.Availabilities?.Select(a => new BoatAvailabilityDto
                {
                    Id = a.Id,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsAvailable = a.IsAvailable,
                    Reason = a.Reason
                }).ToList() ?? new List<BoatAvailabilityDto>(),
                Reviews = b.Reviews?.Select(r => new BoatReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.UserName,
                    UserAvatar = r.UserAvatar,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).OrderByDescending(r => r.CreatedAt).ToList() ?? new List<BoatReviewDto>()
            };
        }

        public async Task<IEnumerable<BoatDto>> GetBoatsAsync(BoatFilters filters)
        {
            var q = _repo.Query().AsNoTracking()
                .Where(b => b.IsActive && b.IsVerified && !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filters?.Location))
            {
                var pattern = $"%{filters.Location}%";
                q = q.Where(b => EF.Functions.Like(b.Location, pattern));
            }

            if (!string.IsNullOrWhiteSpace(filters?.Type))
            {
                q = q.Where(b => b.Type == filters.Type);
            }

            if (!string.IsNullOrWhiteSpace(filters?.Destination))
            {
                if (int.TryParse(filters.Destination, out var destId))
                {
                    q = q.Where(b => b.DestinationId == destId);
                }
                else
                {
                    var pattern = $"%{filters.Destination}%";
                    q = q.Where(b => EF.Functions.Like(b.City, pattern) || EF.Functions.Like(b.Location, pattern));
                }
            }

            if (filters?.PriceMin != null)
                q = q.Where(b => b.Price >= filters.PriceMin.Value);

            if (filters?.PriceMax != null)
                q = q.Where(b => b.Price <= filters.PriceMax.Value);

            if (filters?.CapacityMin != null)
                q = q.Where(b => b.Capacity >= filters.CapacityMin.Value);

            if (!string.IsNullOrWhiteSpace(filters?.StartDate) && !string.IsNullOrWhiteSpace(filters?.EndDate))
            {
                if (DateTime.TryParse(filters.StartDate, out var start) && DateTime.TryParse(filters.EndDate, out var end))
                {
                    q = q.Where(b => b.Availabilities.Any(a => a.StartDate <= start && a.EndDate >= end));
                }
            }

            var list = await q
                .AsNoTracking()
                .ToListAsync();

            return list.Select(MapToDto);
        }

        public async Task<BoatDto?> GetByIdAsync(int id)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null) return null;
            // Only return boats that are active, verified and not deleted
            if (b.IsDeleted) return null;
            return MapToDto(b);
        }

        public async Task<BoatDto> CreateAsync(CreateBoatDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var boat = new Boat
            {
                Name = dto.Name,
                Type = dto.Type,
                Location = dto.Location,
                City = dto.City,
                Country = dto.Country,
                DestinationId = string.IsNullOrWhiteSpace(dto.Destination) ? null : (int?) (int.TryParse(dto.Destination, out var d) ? d : (int?)null),
                Price = dto.Price,
                Capacity = dto.Capacity,
                Cabins = dto.Cabins,
                Length = dto.Length,
                Year = dto.Year,
                Image = dto.Image,
                Equipment = dto.Equipment != null ? string.Join(',', dto.Equipment) : null,
                Description = dto.Description,
                OwnerId = dto.OwnerId,
                IsActive = true,
                IsVerified = false,
                ReviewCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Attach images if provided in DTO
            if (dto.Images != null && dto.Images.Length > 0)
            {
                boat.Images = dto.Images.Select((url, idx) => new BoatImage
                {
                    ImageUrl = url,
                    DisplayOrder = idx,
                    CreatedAt = DateTime.UtcNow
                }).ToList();
            }

            await _repo.AddAsync(boat);
            await _repo.SaveChangesAsync();

            // reload with navigations
            var created = await _repo.GetByIdAsync(boat.Id);
            return MapToDto(created!);
        }

        public async Task<BoatDto> UpdateAsync(int id, UpdateBoatDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            // Get tracked entity so changes are persisted
            var existing = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) throw new KeyNotFoundException($"Boat {id} not found.");

            existing.Name = dto.Name;
            existing.Type = dto.Type;
            existing.Location = dto.Location;
            existing.City = dto.City;
            existing.Country = dto.Country;
            existing.Price = dto.Price;
            existing.Capacity = dto.Capacity;
            existing.Cabins = dto.Cabins;
            existing.Length = dto.Length;
            existing.Year = dto.Year;
            existing.Image = dto.Image;
            existing.Equipment = dto.Equipment != null ? string.Join(',', dto.Equipment) : existing.Equipment;
            existing.Description = dto.Description;
            existing.DestinationId = string.IsNullOrWhiteSpace(dto.Destination) ? existing.DestinationId : (int?) (int.TryParse(dto.Destination, out var d) ? d : existing.DestinationId);
            existing.OwnerId = dto.OwnerId;

            await _repo.SaveChangesAsync();

            var updated = await _repo.GetByIdAsync(id);
            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // obtain tracked entity to persist changes
            var existing = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return false;
            // Soft-delete: mark as deleted instead of removing the record
            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BoatDto>> GetByOwnerAsync(Guid ownerId)
        {
            var q = _repo.Query().Where(b => b.OwnerId == ownerId && !b.IsDeleted).AsNoTracking();
            var list = await q.ToListAsync();
            return list.Select(MapToDto);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            var existing = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return false;
            existing.IsActive = isActive;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetVerifiedAsync(int id, bool isVerified)
        {
            var existing = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return false;
            existing.IsVerified = isVerified;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
            return true;
        }
    }
 }