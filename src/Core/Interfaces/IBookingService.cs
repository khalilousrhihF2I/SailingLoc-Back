using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDto>> GetBookingsAsync(BookingFilters filters);
        Task<BookingDto?> GetBookingByIdAsync(string id);
        Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
        Task<BookingDto> UpdateBookingAsync(string id, UpdateBookingDto dto);
        Task<bool> CancelBookingAsync(string id);
        Task<IEnumerable<BookingDto>> GetBookingsByRenterAsync(Guid renterId);
        Task<IEnumerable<BookingDto>> GetBookingsByOwnerAsync(Guid ownerId);
    }
}
