using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Booking
    {
        public string Id { get; set; } = "";
        public int BoatId { get; set; }
        public Guid RenterId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal DailyPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "";
        public string RenterName { get; set; } = "";
        public string RenterEmail { get; set; } = "";
        public string? RenterPhone { get; set; }

        public string? PaymentIntentId { get; set; }
        public string PaymentStatus { get; set; } = "";
        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Boat Boat { get; set; } = null!;
        public AppUser Renter { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
