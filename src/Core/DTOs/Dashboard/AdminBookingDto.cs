using System;

namespace Core.DTOs.Dashboard
{
    public class AdminBookingDto
    {
        public string Id { get; set; } = string.Empty;
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public Guid RenterId { get; set; }
        public string RenterName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
