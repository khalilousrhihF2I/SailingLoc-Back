using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Notifications;
using Core.Models.Templates;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Services;

/// <summary>
/// Tests unitaires pour BookingService — réservations, annulations, validations.
/// </summary>
public class BookingServiceTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private BookingService _sut = null!;
    private TestSeedData _seed = null!;
    private Mock<IEmailService> _emailMock = null!;

    public async Task InitializeAsync()
    {
        _db = TestDbContextFactory.Create();
        _seed = await TestDbContextFactory.SeedStandardDataAsync(_db);
        var repo = new BookingRepository(_db);
        _emailMock = new Mock<IEmailService>();
        _emailMock.Setup(x => x.SendReservationCreatedEmailAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<ReservationTemplateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _emailMock.Setup(x => x.SendReservationApprovedEmailAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<ReservationApprovedTemplateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _emailMock.Setup(x => x.SendCancellationRequestEmailAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationRequestTemplateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _sut = new BookingService(_db, repo, _emailMock.Object);
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    // ─── GetBookingsAsync ───

    [Fact]
    public async Task GetBookingsAsync_NoFilters_ReturnsAll()
    {
        var result = (await _sut.GetBookingsAsync(new BookingFilters())).Items.ToList();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBookingsAsync_FilterByRenter_ReturnsOnly()
    {
        var result = (await _sut.GetBookingsAsync(new BookingFilters { RenterId = _seed.Renter.Id })).Items.ToList();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.RenterId == _seed.Renter.Id);
    }

    [Fact]
    public async Task GetBookingsAsync_FilterByStatus_ReturnsMatching()
    {
        var result = (await _sut.GetBookingsAsync(new BookingFilters { Status = "pending" })).Items.ToList();
        result.Should().HaveCount(1);
        result[0].Status.Should().Be("pending");
    }

    [Fact]
    public async Task GetBookingsAsync_FilterByOwner_ReturnsMatching()
    {
        var result = (await _sut.GetBookingsAsync(new BookingFilters { OwnerId = _seed.Owner.Id })).Items.ToList();
        result.Should().HaveCount(2);
    }

    // ─── GetBookingByIdAsync ───

    [Fact]
    public async Task GetBookingByIdAsync_Existing_ReturnsDto()
    {
        var result = await _sut.GetBookingByIdAsync(_seed.Booking1.Id);

        result.Should().NotBeNull();
        result!.BoatName.Should().NotBeEmpty();
        result.RenterName.Should().Be("Marie Martin");
    }

    [Fact]
    public async Task GetBookingByIdAsync_NonExisting_ReturnsNull()
    {
        var result = await _sut.GetBookingByIdAsync("NONEXISTENT");
        result.Should().BeNull();
    }

    // ─── CreateBookingAsync ───

    [Fact]
    public async Task CreateBookingAsync_ValidDto_CreatesBooking()
    {
        var dto = new CreateBookingDto
        {
            BoatId = 2,
            RenterId = _seed.Renter.Id,
            StartDate = DateTime.UtcNow.AddDays(50),
            EndDate = DateTime.UtcNow.AddDays(55),
            DailyPrice = 400m,
            ServiceFee = 75m,
            RenterName = "Marie Martin",
            RenterEmail = "renter@test.com"
        };

        var result = await _sut.CreateBookingAsync(dto);

        result.Should().NotBeNull();
        result.Status.Should().Be("pending");
        result.BoatId.Should().Be(2);
        result.Id.Should().StartWith("BK");

        // Verify availability was created as blocked
        var avail = await _db.BoatAvailabilities.FirstOrDefaultAsync(a => a.ReferenceId == result.Id);
        avail.Should().NotBeNull();
        avail!.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task CreateBookingAsync_NonExistingBoat_ThrowsKeyNotFound()
    {
        var dto = new CreateBookingDto
        {
            BoatId = 9999,
            RenterId = _seed.Renter.Id,
            StartDate = DateTime.UtcNow.AddDays(50),
            EndDate = DateTime.UtcNow.AddDays(55),
            DailyPrice = 100m,
            ServiceFee = 10m,
            RenterName = "Test",
            RenterEmail = "test@test.com"
        };

        var act = () => _sut.CreateBookingAsync(dto);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateBookingAsync_BlockedPeriod_ThrowsInvalidOperation()
    {
        // Boat1 has a blocked period starting +30 days for 7 days
        var dto = new CreateBookingDto
        {
            BoatId = 1,
            RenterId = _seed.Renter.Id,
            StartDate = DateTime.UtcNow.AddDays(31),
            EndDate = DateTime.UtcNow.AddDays(35),
            DailyPrice = 250m,
            ServiceFee = 50m,
            RenterName = "Marie Martin",
            RenterEmail = "renter@test.com"
        };

        var act = () => _sut.CreateBookingAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateBookingAsync_OverlappingBooking_ThrowsInvalidOperation()
    {
        // Booking1 on boat1 starts +10 days, ends +17 days
        var dto = new CreateBookingDto
        {
            BoatId = 1,
            RenterId = Guid.NewGuid(), // different renter to avoid renter overlap check
            StartDate = DateTime.UtcNow.AddDays(12),
            EndDate = DateTime.UtcNow.AddDays(15),
            DailyPrice = 250m,
            ServiceFee = 50m,
            RenterName = "Autre",
            RenterEmail = "autre@test.com"
        };

        var act = () => _sut.CreateBookingAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateBookingAsync_RenterOverlapping_ThrowsInvalidOperation()
    {
        // Renter already has booking2 on boat2: +20 to +25 days
        var dto = new CreateBookingDto
        {
            BoatId = 1,
            RenterId = _seed.Renter.Id,
            StartDate = DateTime.UtcNow.AddDays(22),
            EndDate = DateTime.UtcNow.AddDays(24),
            DailyPrice = 250m,
            ServiceFee = 50m,
            RenterName = "Marie Martin",
            RenterEmail = "renter@test.com"
        };

        var act = () => _sut.CreateBookingAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateBookingAsync_SendsEmail()
    {
        var dto = new CreateBookingDto
        {
            BoatId = 2,
            RenterId = _seed.Renter.Id,
            StartDate = DateTime.UtcNow.AddDays(60),
            EndDate = DateTime.UtcNow.AddDays(65),
            DailyPrice = 400m,
            ServiceFee = 75m,
            RenterName = "Marie Martin",
            RenterEmail = "renter@test.com"
        };

        await _sut.CreateBookingAsync(dto);

        _emailMock.Verify(x => x.SendReservationCreatedEmailAsync(
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<ReservationTemplateModel>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── UpdateBookingAsync ───

    [Fact]
    public async Task UpdateBookingAsync_ChangeStatus_UpdatesAndReturns()
    {
        var result = await _sut.UpdateBookingAsync(_seed.Booking2.Id, new UpdateBookingDto
        {
            Id = _seed.Booking2.Id,
            Status = "confirmed"
        });

        result.Should().NotBeNull();
        result.Status.Should().Be("confirmed");
    }

    [Fact]
    public async Task UpdateBookingAsync_NonExisting_ThrowsKeyNotFound()
    {
        var act = () => _sut.UpdateBookingAsync("FAKE", new UpdateBookingDto { Id = "FAKE", Status = "confirmed" });
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── CancelBookingAsync ───

    [Fact]
    public async Task CancelBookingAsync_Existing_CancelsAndRemovesAvailability()
    {
        // First create a booking to have matching availability
        var dto = new CreateBookingDto
        {
            BoatId = 2,
            RenterId = _seed.Renter.Id,
            StartDate = DateTime.UtcNow.AddDays(70),
            EndDate = DateTime.UtcNow.AddDays(75),
            DailyPrice = 400m,
            ServiceFee = 75m,
            RenterName = "Marie Martin",
            RenterEmail = "renter@test.com"
        };
        var created = await _sut.CreateBookingAsync(dto);

        var ok = await _sut.CancelBookingAsync(created.Id);
        ok.Should().BeTrue();

        var booking = await _db.Bookings.FindAsync(created.Id);
        booking!.Status.Should().Be("cancelled");
        booking.CancelledAt.Should().NotBeNull();

        // Availability reference should be removed
        var avail = await _db.BoatAvailabilities.FirstOrDefaultAsync(a => a.ReferenceId == created.Id);
        avail.Should().BeNull();
    }

    [Fact]
    public async Task CancelBookingAsync_NonExisting_ReturnsFalse()
    {
        var ok = await _sut.CancelBookingAsync("NONEXISTENT");
        ok.Should().BeFalse();
    }

    // ─── GetBookingsByRenterAsync ───

    [Fact]
    public async Task GetBookingsByRenterAsync_ReturnsAll()
    {
        var result = (await _sut.GetBookingsByRenterAsync(_seed.Renter.Id)).ToList();
        result.Should().HaveCountGreaterOrEqualTo(2);
    }

    // ─── GetBookingsByOwnerAsync ───

    [Fact]
    public async Task GetBookingsByOwnerAsync_ReturnsAll()
    {
        var result = (await _sut.GetBookingsByOwnerAsync(_seed.Owner.Id)).ToList();
        result.Should().HaveCountGreaterOrEqualTo(2);
    }
}
