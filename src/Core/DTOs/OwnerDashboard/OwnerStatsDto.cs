namespace Core.DTOs.OwnerDashboard
{
    public class OwnerStatsDto
    {
        public int BoatsCount { get; set; }
        public int BookingsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingBookings { get; set; }
        public double OccupancyRate { get; set; }
    }
}
