using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IAvailabilityService
    {
        Task<AvailabilityCheck> CheckAvailabilityAsync(int boatId, DateTime start, DateTime end, string? excludeBookingId);
        Task<IEnumerable<UnavailablePeriod>> GetUnavailableDatesAsync(int boatId, DateTime? start, DateTime? end);
        Task<UnavailablePeriod> AddUnavailablePeriodAsync(int boatId, AddUnavailablePeriodDto dto);
        Task<bool> RemoveUnavailablePeriodAsync(int boatId, DateTime start);
        Task<bool> BlockPeriodAsync(CreateAvailabilityDto dto);
        Task<bool> UnblockPeriodAsync(int availabilityId);
    }
}
