using System;

namespace Core.DTOs.Dashboard
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Verified { get; set; }
        public int BoatsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string MemberSince { get; set; } = string.Empty;
    }
}
