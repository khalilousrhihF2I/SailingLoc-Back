using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Helpers;

/// <summary>
/// Factory to create an in-memory ApplicationDbContext for unit tests.
/// Ensures a unique database name per test so tests are fully isolated.
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    /// <summary>
    /// Seeds standard test data: roles, users, boats, destinations, bookings, reviews.
    /// </summary>
    public static async Task<TestSeedData> SeedStandardDataAsync(ApplicationDbContext db)
    {
        var seed = new TestSeedData();

        // Roles
        var adminRole = new AppRole { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN" };
        var ownerRole = new AppRole { Id = Guid.NewGuid(), Name = "Owner", NormalizedName = "OWNER" };
        var renterRole = new AppRole { Id = Guid.NewGuid(), Name = "Renter", NormalizedName = "RENTER" };
        db.Roles.AddRange(adminRole, ownerRole, renterRole);

        // Users
        var owner = new AppUser
        {
            Id = Guid.NewGuid(),
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "owner@test.com",
            NormalizedEmail = "OWNER@TEST.COM",
            UserName = "owner@test.com",
            NormalizedUserName = "OWNER@TEST.COM",
            UserType = "owner",
            Verified = true,
            MemberSince = DateTime.UtcNow.AddMonths(-6),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var renter = new AppUser
        {
            Id = Guid.NewGuid(),
            FirstName = "Marie",
            LastName = "Martin",
            Email = "renter@test.com",
            NormalizedEmail = "RENTER@TEST.COM",
            UserName = "renter@test.com",
            NormalizedUserName = "RENTER@TEST.COM",
            UserType = "renter",
            Verified = true,
            MemberSince = DateTime.UtcNow.AddMonths(-3),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@test.com",
            NormalizedEmail = "ADMIN@TEST.COM",
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM",
            UserType = "admin",
            Verified = true,
            MemberSince = DateTime.UtcNow.AddYears(-1),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        db.Users.AddRange(owner, renter, admin);

        // UserRoles
        db.UserRoles.AddRange(
            new IdentityUserRole<Guid> { UserId = owner.Id, RoleId = ownerRole.Id },
            new IdentityUserRole<Guid> { UserId = renter.Id, RoleId = renterRole.Id },
            new IdentityUserRole<Guid> { UserId = admin.Id, RoleId = adminRole.Id }
        );

        // Destinations
        var dest1 = new Destination
        {
            Id = 1,
            Name = "Côte d'Azur",
            Region = "Méditerranée",
            Country = "France",
            Description = "French Riviera",
            BoatCount = 2,
            CreatedAt = DateTime.UtcNow
        };
        var dest2 = new Destination
        {
            Id = 2,
            Name = "Bretagne",
            Region = "Atlantique",
            Country = "France",
            Description = "Brittany coast",
            BoatCount = 1,
            CreatedAt = DateTime.UtcNow
        };
        db.Destinations.AddRange(dest1, dest2);

        // Boats
        var boat1 = new Boat
        {
            Id = 1,
            Name = "Voilier Azur",
            Type = "Voilier",
            Location = "Nice",
            City = "Nice",
            Country = "France",
            DestinationId = 1,
            Price = 250m,
            Capacity = 6,
            Cabins = 2,
            Length = 12.5m,
            Year = 2020,
            Rating = 4.5m,
            ReviewCount = 2,
            OwnerId = owner.Id,
            IsActive = true,
            IsVerified = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };
        var boat2 = new Boat
        {
            Id = 2,
            Name = "Catamaran Breizh",
            Type = "Catamaran",
            Location = "Saint-Malo",
            City = "Saint-Malo",
            Country = "France",
            DestinationId = 2,
            Price = 400m,
            Capacity = 8,
            Cabins = 4,
            Length = 15m,
            Year = 2022,
            Rating = 0,
            ReviewCount = 0,
            OwnerId = owner.Id,
            IsActive = true,
            IsVerified = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };
        var boat3 = new Boat
        {
            Id = 3,
            Name = "Deleted Boat",
            Type = "Voilier",
            Location = "Marseille",
            City = "Marseille",
            Country = "France",
            Price = 100m,
            Capacity = 4,
            Cabins = 1,
            Length = 8m,
            Year = 2015,
            OwnerId = owner.Id,
            IsActive = true,
            IsVerified = true,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-5)
        };
        db.Boats.AddRange(boat1, boat2, boat3);

        // BoatImages
        db.BoatImages.AddRange(
            new BoatImage { Id = 1, BoatId = 1, ImageUrl = "/img/boat1-1.jpg", DisplayOrder = 0, CreatedAt = DateTime.UtcNow },
            new BoatImage { Id = 2, BoatId = 1, ImageUrl = "/img/boat1-2.jpg", DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
            new BoatImage { Id = 3, BoatId = 2, ImageUrl = "/img/boat2-1.jpg", DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
        );

        // BoatAvailability — a blocked period on boat1
        db.BoatAvailabilities.Add(new BoatAvailability
        {
            Id = 1,
            BoatId = 1,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(37),
            IsAvailable = false,
            ReferenceType = "blocked",
            Reason = "Maintenance",
            CreatedAt = DateTime.UtcNow
        });

        // Bookings
        var booking1 = new Booking
        {
            Id = "BK20260101-abc12345",
            BoatId = 1,
            RenterId = renter.Id,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(17),
            DailyPrice = 250m,
            Subtotal = 1750m,
            ServiceFee = 50m,
            TotalPrice = 1800m,
            Status = "confirmed",
            RenterName = "Marie Martin",
            RenterEmail = "renter@test.com",
            RenterPhone = "+33600000001",
            PaymentStatus = "succeeded",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        var booking2 = new Booking
        {
            Id = "BK20260102-def67890",
            BoatId = 2,
            RenterId = renter.Id,
            StartDate = DateTime.UtcNow.AddDays(20),
            EndDate = DateTime.UtcNow.AddDays(25),
            DailyPrice = 400m,
            Subtotal = 2000m,
            ServiceFee = 75m,
            TotalPrice = 2075m,
            Status = "pending",
            RenterName = "Marie Martin",
            RenterEmail = "renter@test.com",
            PaymentStatus = "succeeded",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        db.Bookings.AddRange(booking1, booking2);

        // Reviews
        var review1 = new Review
        {
            Id = 1,
            BoatId = 1,
            UserId = renter.Id,
            BookingId = booking1.Id,
            UserName = "Marie Martin",
            Rating = 5,
            Comment = "Excellent bateau !",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var review2 = new Review
        {
            Id = 2,
            BoatId = 1,
            UserId = renter.Id,
            UserName = "Marie Martin",
            Rating = 4,
            Comment = "Très bien",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        db.Reviews.AddRange(review1, review2);

        await db.SaveChangesAsync();

        seed.Owner = owner;
        seed.Renter = renter;
        seed.Admin = admin;
        seed.Boat1 = boat1;
        seed.Boat2 = boat2;
        seed.DeletedBoat = boat3;
        seed.Destination1 = dest1;
        seed.Destination2 = dest2;
        seed.Booking1 = booking1;
        seed.Booking2 = booking2;
        seed.Review1 = review1;
        seed.Review2 = review2;

        return seed;
    }
}

public class TestSeedData
{
    public AppUser Owner { get; set; } = null!;
    public AppUser Renter { get; set; } = null!;
    public AppUser Admin { get; set; } = null!;
    public Boat Boat1 { get; set; } = null!;
    public Boat Boat2 { get; set; } = null!;
    public Boat DeletedBoat { get; set; } = null!;
    public Destination Destination1 { get; set; } = null!;
    public Destination Destination2 { get; set; } = null!;
    public Booking Booking1 { get; set; } = null!;
    public Booking Booking2 { get; set; } = null!;
    public Review Review1 { get; set; } = null!;
    public Review Review2 { get; set; } = null!;
}
