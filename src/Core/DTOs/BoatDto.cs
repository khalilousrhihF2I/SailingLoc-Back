using Core.Entities;
using System;
using System.Collections.Generic;

namespace Core.DTOs
{
    public class BoatImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class BoatAvailabilityDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
    }

    public class BoatOwnerDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class BoatReviewDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BoatDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int? DestinationId { get; set; }
        public string Country { get; set; } = string.Empty;
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

        public BoatOwnerDto Owner { get; set; } = new BoatOwnerDto();

        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<BoatImageDto> Images { get; set; } = new();
        public List<BoatAvailabilityDto> Availabilities { get; set; } = new();
        public List<BoatReviewDto> Reviews { get; set; } = new();
        public List<UserDocument> Documents { get; set; } = new();
    }
}
