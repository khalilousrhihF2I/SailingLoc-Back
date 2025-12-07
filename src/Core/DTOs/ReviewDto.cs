using System;

namespace Core.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int BoatId { get; set; }
        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string Date { get; set; } = string.Empty; // yyyy-MM-dd
    }
}
