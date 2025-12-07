using Core.DTOs.Home;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IHomeService
    {
        Task<HomeDto> GetHomeDataAsync();
    }
}
