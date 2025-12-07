namespace Core.DTOs.Dashboard
{
    public class AdminPaymentStatsDto
    {
        public decimal Paid { get; set; }
        public decimal Pending { get; set; }
        public decimal PlatformFee { get; set; }
    }
}
