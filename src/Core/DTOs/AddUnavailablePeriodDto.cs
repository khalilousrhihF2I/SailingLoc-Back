using System;

namespace Core.DTOs
{
    public class AddUnavailablePeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; } = "blocked"; // only blocked supported for adding
        public string? Reason { get; set; }
    }
}
