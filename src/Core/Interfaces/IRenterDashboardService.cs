using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs.RenterDashboard;

namespace Core.Interfaces
{
    public interface IRenterDashboardService
    {
        Task<RenterStatsDto> GetStatsAsync(Guid userId);
        Task<List<RenterBookingDto>> GetBookingsAsync(Guid userId);
        Task<RenterProfileDto> GetProfileAsync(Guid userId);
        Task<RenterProfileDto> UpdateProfileAsync(Guid userId, RenterProfileDto updated);
        Task<List<RenterDocumentDto>> GetDocumentsAsync(Guid userId);
        Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(Guid userId);
    }
}
