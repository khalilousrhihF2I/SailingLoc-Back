using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Services;

/// <summary>
/// Tests unitaires pour AvailabilityService — vérification de disponibilité, blocage/déblocage.
/// </summary>
public class AvailabilityServiceTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private AvailabilityService _sut = null!;
    private TestSeedData _seed = null!;

    public async Task InitializeAsync()
    {
        _db = TestDbContextFactory.Create();
        _seed = await TestDbContextFactory.SeedStandardDataAsync(_db);
        var repo = new AvailabilityRepository(_db);
        _sut = new AvailabilityService(_db, repo);
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    // ─── CheckAvailabilityAsync ───

    [Fact]
    public async Task CheckAvailability_FreePeriod_ReturnsAvailable()
    {
        var result = await _sut.CheckAvailabilityAsync(2, DateTime.UtcNow.AddDays(80), DateTime.UtcNow.AddDays(85), null);

        result.IsAvailable.Should().BeTrue();
        result.Message.Should().Contain("Available");
    }

    [Fact]
    public async Task CheckAvailability_BlockedPeriod_ReturnsUnavailable()
    {
        // Boat1 has blocked period: +30 to +37 days
        var result = await _sut.CheckAvailabilityAsync(1, DateTime.UtcNow.AddDays(31), DateTime.UtcNow.AddDays(35), null);

        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("blocked");
    }

    [Fact]
    public async Task CheckAvailability_BookedPeriod_ReturnsUnavailable()
    {
        // Booking1 on boat1: +10 to +17 days
        var result = await _sut.CheckAvailabilityAsync(1, DateTime.UtcNow.AddDays(12), DateTime.UtcNow.AddDays(15), null);

        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("booked");
    }

    [Fact]
    public async Task CheckAvailability_BookedPeriodWithExclude_ReturnsAvailable()
    {
        // exclude the booking
        var result = await _sut.CheckAvailabilityAsync(1, DateTime.UtcNow.AddDays(12), DateTime.UtcNow.AddDays(15), _seed.Booking1.Id);

        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAvailability_InvalidDateRange_ReturnsUnavailable()
    {
        var result = await _sut.CheckAvailabilityAsync(1, DateTime.UtcNow.AddDays(20), DateTime.UtcNow.AddDays(10), null);

        result.IsAvailable.Should().BeFalse();
        result.Message.Should().Contain("Invalid");
    }

    // ─── GetUnavailableDatesAsync ───

    [Fact]
    public async Task GetUnavailableDates_ReturnsBlockedAndBookings()
    {
        var result = (await _sut.GetUnavailableDatesAsync(1, null, null)).ToList();

        // should include 1 blocked period + at least 1 booking
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Should().Contain(p => p.Type == "blocked");
        result.Should().Contain(p => p.Type == "booking");
    }

    [Fact]
    public async Task GetUnavailableDates_WithDateRange_FiltersCorrectly()
    {
        // Use a far-future range where no blocked availability exists
        var far = DateTime.UtcNow.AddYears(5);
        var result = (await _sut.GetUnavailableDatesAsync(1, far, far.AddDays(10))).ToList();

        // BoatAvailability records are filtered by date range so none should match,
        // but bookings are always included regardless of date range (service behavior).
        result.Should().AllSatisfy(p => p.Type.Should().Be("booking"));
    }

    // ─── AddUnavailablePeriodAsync ───

    [Fact]
    public async Task AddUnavailablePeriod_ValidDto_ReturnsCreated()
    {
        var dto = new AddUnavailablePeriodDto
        {
            StartDate = DateTime.UtcNow.AddDays(90),
            EndDate = DateTime.UtcNow.AddDays(95),
            Type = "maintenance",
            Reason = "Annual service"
        };

        var result = await _sut.AddUnavailablePeriodAsync(1, dto);

        result.Should().NotBeNull();
        result.Type.Should().Be("maintenance");
        result.Reason.Should().Be("Annual service");
    }

    [Fact]
    public async Task AddUnavailablePeriod_NullDto_ThrowsArgumentNull()
    {
        var act = () => _sut.AddUnavailablePeriodAsync(1, null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddUnavailablePeriod_InvalidDateRange_ThrowsArgument()
    {
        var dto = new AddUnavailablePeriodDto
        {
            StartDate = DateTime.UtcNow.AddDays(95),
            EndDate = DateTime.UtcNow.AddDays(90)
        };

        var act = () => _sut.AddUnavailablePeriodAsync(1, dto);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ─── RemoveUnavailablePeriodAsync ───

    [Fact]
    public async Task RemoveUnavailablePeriod_Existing_ReturnsTrue()
    {
        // The seeded blocked period starts at +30 days
        var startDate = _db.BoatAvailabilities.First(a => a.BoatId == 1 && a.Reason == "Maintenance").StartDate;
        var ok = await _sut.RemoveUnavailablePeriodAsync(1, startDate);
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUnavailablePeriod_NonExisting_ReturnsFalse()
    {
        var ok = await _sut.RemoveUnavailablePeriodAsync(1, DateTime.UtcNow.AddDays(999));
        ok.Should().BeFalse();
    }

    // ─── BlockPeriodAsync ───

    [Fact]
    public async Task BlockPeriod_ValidDto_ReturnsTrue()
    {
        var dto = new CreateAvailabilityDto
        {
            BoatId = 2,
            StartDate = DateTime.UtcNow.AddDays(80),
            EndDate = DateTime.UtcNow.AddDays(85),
            IsAvailable = false,
            Reason = "Owner block"
        };

        var ok = await _sut.BlockPeriodAsync(dto);
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task BlockPeriod_InvalidDateRange_ReturnsFalse()
    {
        var dto = new CreateAvailabilityDto
        {
            BoatId = 2,
            StartDate = DateTime.UtcNow.AddDays(85),
            EndDate = DateTime.UtcNow.AddDays(80),
            IsAvailable = false
        };

        var ok = await _sut.BlockPeriodAsync(dto);
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task BlockPeriod_NullDto_ThrowsArgumentNull()
    {
        var act = () => _sut.BlockPeriodAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ─── UnblockPeriodAsync ───

    [Fact]
    public async Task UnblockPeriod_Existing_ReturnsTrue()
    {
        var ok = await _sut.UnblockPeriodAsync(1);
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task UnblockPeriod_NonExisting_ReturnsFalse()
    {
        var ok = await _sut.UnblockPeriodAsync(9999);
        ok.Should().BeFalse();
    }
}
