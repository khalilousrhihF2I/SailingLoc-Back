using System;

namespace Core.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int BoatId { get; set; }
        public string? BookingId { get; set; }
        public Guid UserId { get; set; }

        public string UserName { get; set; } = "";
        public string? UserAvatar { get; set; }

        public int Rating { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Boat Boat { get; set; } = null!;
        public Booking? Booking { get; set; }
        public AppUser User { get; set; } = null!;
    }
}
