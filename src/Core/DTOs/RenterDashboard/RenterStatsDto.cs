namespace Core.DTOs.RenterDashboard
{
    public class RenterStatsDto
    {
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
