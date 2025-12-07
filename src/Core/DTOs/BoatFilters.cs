 
using System;

namespace Core.DTOs
{
    public class BoatFilters
    {
        public string? Location { get; set; }
        public string? Destination { get; set; } // front fournit string; service tente parse en int si c'est un id
        public string? Type { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public int? CapacityMin { get; set; }
        public string? StartDate { get; set; } // ISO string attendu depuis le front
        public string? EndDate { get; set; }
    }
}