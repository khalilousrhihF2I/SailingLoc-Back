using System;

namespace Core.DTOs
{
    public class CreateAvailabilityDto
    {
        public int BoatId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
    }
}
