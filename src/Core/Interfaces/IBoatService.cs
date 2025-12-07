using Core.DTOs;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IBoatService
    {
        Task<IEnumerable<BoatDto>> GetBoatsAsync(BoatFilters filters);
        Task<BoatDto?> GetByIdAsync(int id);
        Task<BoatDto> CreateAsync(CreateBoatDto dto);
        Task<BoatDto> UpdateAsync(int id, UpdateBoatDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<BoatDto>> GetByOwnerAsync(Guid ownerId);
        Task<bool> SetActiveAsync(int id, bool isActive);
        Task<bool> SetVerifiedAsync(int id, bool isVerified);
    }
}