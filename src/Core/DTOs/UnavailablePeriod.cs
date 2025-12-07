using System;

namespace Core.DTOs
{
    public class UnavailablePeriod
    {
        public string Type { get; set; } = null!; // "booking" | "blocked"
        public string? ReferenceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public string? Details { get; set; }
    }
}
