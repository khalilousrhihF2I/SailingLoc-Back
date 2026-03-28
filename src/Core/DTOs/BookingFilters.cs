using System;

namespace Core.DTOs
{
    public class BookingFilters
    {
        public Guid? RenterId { get; set; }
        public Guid? OwnerId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
