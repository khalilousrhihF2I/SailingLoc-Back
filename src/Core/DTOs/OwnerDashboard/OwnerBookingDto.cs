using System;

namespace Core.DTOs.OwnerDashboard
{
    public class OwnerBookingDto
    {
        public string Id { get; set; } = string.Empty;
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public string BoatType { get; set; } = string.Empty;
        public string? BoatImage { get; set; }
        public Guid RenterId { get; set; }
        public string RenterName { get; set; } = string.Empty;
        public string? RenterPhone { get; set; }
        public string? RenterEmail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BoatPricePerDay { get; set; }
        public decimal OwnerRevenue { get; set; } // after commission (90% by default)
        public string PaymentStatus { get; set; } = "pending";
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
