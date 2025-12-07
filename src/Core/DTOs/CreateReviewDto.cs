using System;

namespace Core.DTOs
{
    public class CreateReviewDto
    {
        public int BoatId { get; set; }
        public string? BookingId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
