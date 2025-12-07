using System;

namespace Core.Entities
{
    public class BoatImage
    {
        public int Id { get; set; }
        public int BoatId { get; set; }
        public string ImageUrl { get; set; } = "";
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }

        public Boat Boat { get; set; } = null!;
    }
}
