using System;

namespace Core.DTOs
{
    public class BookingDto
    {
        public string Id { get; set; } = string.Empty;
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public string? BoatImage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ServiceFee { get; set; }
        public string Status { get; set; } = string.Empty; // pending | confirmed | completed | cancelled
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string OwnerPhoneNumber { get; set; } = string.Empty;
        public Guid RenterId { get; set; }
        public string RenterName { get; set; } = string.Empty;
        public string RenterEmail { get; set; } = string.Empty;
        public string RenterPhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
