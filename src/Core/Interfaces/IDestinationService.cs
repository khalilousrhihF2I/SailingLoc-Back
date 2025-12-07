using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IDestinationService
    {
        Task<IEnumerable<Destination>> GetAllAsync();
        Task<Destination?> GetByIdAsync(int id);
        Task<IEnumerable<Destination>> SearchAsync(string query);
        Task<IEnumerable<Destination>> GetByRegionAsync(string region);
        Task<IEnumerable<Destination>> GetPopularAsync(int limit = 4);
        Task<Destination> CreateAsync(Destination dto);
        Task<Destination> UpdateAsync(int id, Destination dto);
        Task<bool> DeleteAsync(int id);
    }
}
