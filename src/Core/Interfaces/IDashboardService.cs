using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs.Dashboard;

namespace Core.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminStatsDto> GetStatsAsync();
        Task<List<AdminUserDto>> GetUsersAsync();
        Task<List<AdminBoatDto>> GetBoatsAsync();
        Task<List<AdminBookingDto>> GetBookingsAsync();
        Task<List<AdminActivityDto>> GetActivityAsync();
        Task<AdminPaymentStatsDto> GetPaymentStatsAsync();
    }
}
