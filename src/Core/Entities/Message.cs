using System;

namespace Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string? BookingId { get; set; }
        public int? BoatId { get; set; }

        public string? Subject { get; set; }
        public string Content { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public AppUser Sender { get; set; } = null!;
        public AppUser Receiver { get; set; } = null!;
        public Booking? Booking { get; set; }
        public Boat? Boat { get; set; }
    }
}
