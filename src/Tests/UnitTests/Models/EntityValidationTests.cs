using Core.Entities;
using Core.DTOs;
using FluentAssertions;
using Xunit;

namespace Tests.UnitTests.Models;

/// <summary>
/// Tests de validation des entités et DTOs — valeurs par défaut, contraintes métier.
/// </summary>
public class EntityValidationTests
{
    // ─── Boat Entity ───

    [Fact]
    public void Boat_DefaultValues_AreCorrect()
    {
        var boat = new Boat();

        boat.Name.Should().BeEmpty();
        boat.IsActive.Should().BeFalse();
        boat.IsVerified.Should().BeFalse();
        boat.IsDeleted.Should().BeFalse();
        boat.Rating.Should().Be(0);
        boat.ReviewCount.Should().Be(0);
        boat.Images.Should().BeEmpty();
        boat.Availabilities.Should().BeEmpty();
        boat.Bookings.Should().BeEmpty();
        boat.Reviews.Should().BeEmpty();
    }

    // ─── Booking Entity ───

    [Fact]
    public void Booking_DefaultValues_AreCorrect()
    {
        var booking = new Booking();

        booking.Id.Should().BeEmpty();
        booking.Status.Should().BeEmpty();
        booking.TotalPrice.Should().Be(0);
        booking.CancelledAt.Should().BeNull();
    }

    [Fact]
    public void Booking_Duration_CalculatesCorrectly()
    {
        var booking = new Booking
        {
            StartDate = new DateTime(2026, 7, 1),
            EndDate = new DateTime(2026, 7, 8)
        };

        var duration = (booking.EndDate - booking.StartDate).TotalDays;
        duration.Should().Be(7);
    }

    // ─── Review Entity ───

    [Fact]
    public void Review_DefaultValues()
    {
        var review = new Review();
        review.Rating.Should().Be(0);
        review.UserName.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Review_ValidRatings(int rating)
    {
        var review = new Review { Rating = rating };
        review.Rating.Should().BeInRange(1, 5);
    }

    // ─── Destination Entity ───

    [Fact]
    public void Destination_DefaultValues()
    {
        var dest = new Destination();
        dest.Name.Should().BeEmpty();
        dest.Boats.Should().BeEmpty();
    }

    // ─── BoatAvailability Entity ───

    [Fact]
    public void BoatAvailability_DefaultValues()
    {
        var avail = new BoatAvailability();
        avail.IsAvailable.Should().BeFalse();
        avail.Reason.Should().BeNull();
    }

    // ─── RefreshToken Entity ───

    [Fact]
    public void RefreshToken_DefaultValues()
    {
        var token = new RefreshToken();
        token.Id.Should().NotBeEmpty();
        token.Revoked.Should().BeFalse();
        token.Token.Should().BeEmpty();
    }

    // ─── AppUser Entity ───

    [Fact]
    public void AppUser_DefaultValues()
    {
        var user = new AppUser();
        user.FirstName.Should().BeEmpty();
        user.LastName.Should().BeEmpty();
        user.Verified.Should().BeFalse();
        user.UserType.Should().BeEmpty();
        user.RefreshTokens.Should().BeEmpty();
        user.BoatsOwned.Should().BeEmpty();
        user.Bookings.Should().BeEmpty();
    }

    // ─── DTO Defaults ───

    [Fact]
    public void BoatDto_DefaultValues()
    {
        var dto = new BoatDto();
        dto.Name.Should().BeEmpty();
        dto.Images.Should().BeEmpty();
        dto.Availabilities.Should().BeEmpty();
        dto.Reviews.Should().BeEmpty();
    }

    [Fact]
    public void BookingDto_DefaultValues()
    {
        var dto = new BookingDto();
        dto.Id.Should().BeEmpty();
        dto.Status.Should().BeEmpty();
    }

    [Fact]
    public void CreateBookingDto_Defaults()
    {
        var dto = new CreateBookingDto();
        dto.BoatId.Should().Be(0);
        dto.RenterName.Should().BeEmpty();
        dto.RenterEmail.Should().BeEmpty();
    }

    [Fact]
    public void BoatFilters_AllNull_ByDefault()
    {
        var filters = new BoatFilters();
        filters.Location.Should().BeNull();
        filters.Type.Should().BeNull();
        filters.PriceMin.Should().BeNull();
        filters.PriceMax.Should().BeNull();
        filters.CapacityMin.Should().BeNull();
    }

    [Fact]
    public void BookingFilters_AllNull_ByDefault()
    {
        var filters = new BookingFilters();
        filters.RenterId.Should().BeNull();
        filters.OwnerId.Should().BeNull();
        filters.Status.Should().BeNull();
        filters.StartDate.Should().BeNull();
        filters.EndDate.Should().BeNull();
    }

    [Fact]
    public void AvailabilityCheck_Defaults()
    {
        var check = new AvailabilityCheck();
        check.IsAvailable.Should().BeFalse();
        check.Message.Should().BeEmpty();
    }

    [Fact]
    public void AddUnavailablePeriodDto_DefaultType()
    {
        var dto = new AddUnavailablePeriodDto();
        dto.Type.Should().Be("blocked");
    }

    [Fact]
    public void UserDto_DefaultValues()
    {
        var dto = new UserDto();
        dto.Name.Should().BeEmpty();
        dto.Email.Should().BeEmpty();
        dto.Documents.Should().BeEmpty();
    }

    [Fact]
    public void ReviewDto_DefaultValues()
    {
        var dto = new ReviewDto();
        dto.UserName.Should().BeEmpty();
        dto.Date.Should().BeEmpty();
    }
}
