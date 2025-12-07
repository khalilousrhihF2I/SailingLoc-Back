using System;

namespace Core.DTOs.OwnerDashboard
{
    public class UpdateAvailabilityDto
    {
        public string Action { get; set; } = "block"; // block | unblock
        public string? ReferenceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
