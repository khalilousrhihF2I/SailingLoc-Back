using System;

namespace Core.DTOs
{
    public class CreateBookingDto
    {
        public int BoatId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DailyPrice { get; set; }
        public decimal ServiceFee { get; set; }
        public Guid RenterId { get; set; }
        public string RenterName { get; set; } = string.Empty;
        public string RenterEmail { get; set; } = string.Empty;
        public string? RenterPhone { get; set; }
    }
}
