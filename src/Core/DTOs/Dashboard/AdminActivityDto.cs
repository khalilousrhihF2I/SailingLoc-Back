using System;

namespace Core.DTOs.Dashboard
{
    public class AdminActivityDto
    {
        public string Type { get; set; } = string.Empty; // "booking" | "boat" | "user" | "login" | "review" | "dispute" | "system"
        public string Description { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string? Action { get; set; }
        public string? UserId { get; set; }
        public string? Ip { get; set; }
    }
}
