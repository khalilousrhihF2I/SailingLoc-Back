using System.Collections.Generic;

namespace Core.DTOs.Home
{
    public class TopBoatTypeDto
    {
        public string Type { get; set; } = "";
        public int Count { get; set; }
    }

    public class PopularBoatDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Location { get; set; } = "";
        public string City { get; set; } = "";
        public int? DestinationId { get; set; }
        public string Country { get; set; } = "";
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int Cabins { get; set; }
        public decimal Length { get; set; }
        public int Year { get; set; }
        public string? Image { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? Equipment { get; set; }
        public string? Description { get; set; }
        public System.Guid OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerAvatar { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
    }

    public class HomeDto
    {
        public List<TopBoatTypeDto> TopBoatTypes { get; set; } = new();
        public List<PopularBoatDto> PopularBoats { get; set; } = new();
    }
}
