using System;

namespace Core.DTOs.Dashboard
{
    public class AdminBoatDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Image { get; set; }  
        public decimal Price { get; set; }
        public bool IsVerified { get; set; }
        public int ReviewCount { get; set; }
    }
}
