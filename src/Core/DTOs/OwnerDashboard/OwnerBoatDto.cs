using System;
using System.Collections.Generic;

namespace Core.DTOs.OwnerDashboard
{
    public class OwnerBoatDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Image { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; }
        public int ReviewCount { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
