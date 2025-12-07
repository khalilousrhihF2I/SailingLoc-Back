namespace Infrastructure.Models.Templates
{
    public class ResetCodeTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string Code { get; set; } = default!;
        public int ExpiresMinutes { get; set; }
        public string? SupportEmail { get; set; }
        public string RecipientName { get; set; } = string.Empty;  
    }
}
