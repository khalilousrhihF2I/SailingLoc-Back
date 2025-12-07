using System;

namespace Core.DTOs.RenterDashboard
{
    public class RenterBookingDto
    {
        public string Id { get; set; } = string.Empty;
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public string? BoatImage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
