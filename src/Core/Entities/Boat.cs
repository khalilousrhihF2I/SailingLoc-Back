using System;
using System.Collections.Generic;

namespace Core.Entities
{
    /// <summary>
    /// Représente un bateau louable sur la plateforme.
    /// </summary>
    public class Boat
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

        public Guid OwnerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public AppUser Owner { get; set; } = null!;
        public Destination? Destination { get; set; }
        public ICollection<BoatImage> Images { get; set; } = new List<BoatImage>();
        public ICollection<BoatAvailability> Availabilities { get; set; } = new List<BoatAvailability>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Message> Messages { get; set; } = new List<Message>(); 
    }
}
