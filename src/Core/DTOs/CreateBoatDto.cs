 
using System;

namespace Core.DTOs; 
    public class CreateBoatDto
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? Destination { get; set; }
        public string Country { get; set; } = null!;
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int Cabins { get; set; }
        public decimal Length { get; set; }
        public int Year { get; set; }
        public string? Image { get; set; }
        public string[]? Equipment { get; set; }
        public string[]? Images { get; set; }
        public string? Description { get; set; }
        public Guid OwnerId { get; set; } // Utiliser Guid pour OwnerId (conforme au domaine)
    } 