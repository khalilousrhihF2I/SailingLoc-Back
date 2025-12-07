using System;
using System.Collections.Generic;

namespace Core.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // renter, owner, admin
        public string? Avatar { get; set; }
        public string? Phone { get; set; }
        public bool Verified { get; set; }
        public List<string> Documents { get; set; } = new List<string>();
        public DateTime MemberSince { get; set; }
        public int BoatsCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}