namespace Core.DTOs.Dashboard
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalBoats { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingVerifications { get; set; }
        public int PendingDocuments { get; set; }
        public int Disputes { get; set; }
    }
}
