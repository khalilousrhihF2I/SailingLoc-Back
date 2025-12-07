using System;

namespace Core.DTOs.Dashboard
{
    public class AdminActivityDto
    {
        public string Type { get; set; } = string.Empty; // "booking" | "boat" | "user"
        public string Description { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
    }
}
