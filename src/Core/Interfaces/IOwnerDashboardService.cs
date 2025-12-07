using Core.DTOs.OwnerDashboard;
using Core.DTOs.RenterDashboard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IOwnerDashboardService
    {
        Task<OwnerStatsDto> GetStatsAsync(Guid ownerId);
        Task<List<OwnerBoatDto>> GetBoatsAsync(Guid ownerId);
        Task<List<OwnerBookingDto>> GetBookingsAsync(Guid ownerId);
        Task<RenterProfileDto> GetProfileAsync(Guid userId);
        Task<RenterProfileDto> UpdateProfileAsync(Guid userId, RenterProfileDto updated);
        Task<OwnerRevenueDto> GetRevenueAsync(Guid ownerId);
        Task<OwnerAvailabilityDto> GetAvailabilityAsync(int boatId, Guid ownerId);
        Task<bool> UpdateAvailabilityAsync(int boatId, Guid ownerId, UpdateAvailabilityDto dto);
    }
}
