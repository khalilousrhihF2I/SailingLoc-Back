using System.Collections.Generic;

namespace Core.DTOs.OwnerDashboard
{
    public class OwnerAvailabilityDto
    {
        public List<OwnerAvailabilityPeriod> Periods { get; set; } = new List<OwnerAvailabilityPeriod>();
    }

    public class OwnerAvailabilityPeriod
    {
        public string Type { get; set; } = string.Empty; // "booking" or "blocked"
        public string ReferenceId { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty; // ISO
        public string EndDate { get; set; } = string.Empty; // ISO
        public string? Reason { get; set; }
    }
}
