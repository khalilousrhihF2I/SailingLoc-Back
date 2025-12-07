using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Region { get; set; } = "";
        public string Country { get; set; } = "";
        public string? Description { get; set; }
        public string? Image { get; set; }

        public decimal AveragePrice { get; set; }
        public string? PopularMonths { get; set; }
        public string? Highlights { get; set; }
        public int BoatCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Boat> Boats { get; set; } = [];
    }
}
