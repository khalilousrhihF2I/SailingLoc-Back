namespace Core.DTOs.RenterDashboard
{
    public class PaymentMethodDto
    {
        public string Id { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public string Expiry { get; set; } = string.Empty;
    }
}
