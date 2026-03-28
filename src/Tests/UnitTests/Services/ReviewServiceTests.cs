using Core.DTOs;
using Core.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Services;

/// <summary>
/// Tests unitaires pour ReviewService — CRUD des avis, moyenne, avis récents.
/// </summary>
public class ReviewServiceTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private ReviewService _sut = null!;
    private TestSeedData _seed = null!;

    public async Task InitializeAsync()
    {
        _db = TestDbContextFactory.Create();
        _seed = await TestDbContextFactory.SeedStandardDataAsync(_db);
        _db.ChangeTracker.Clear(); // detach all so AsNoTracking + Remove pattern works
        var repo = new ReviewRepository(_db);
        _sut = new ReviewService(_db, repo);
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    // ─── GetAllReviewsAsync ───

    [Fact]
    public async Task GetAllReviewsAsync_ReturnsAll()
    {
        var result = await _sut.GetAllReviewsAsync();
        result.Should().HaveCount(2);
    }

    // ─── GetReviewsByBoatIdAsync ───

    [Fact]
    public async Task GetReviewsByBoatId_Boat1_ReturnsTwo()
    {
        var result = await _sut.GetReviewsByBoatIdAsync(1);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetReviewsByBoatId_Boat2_ReturnsEmpty()
    {
        var result = await _sut.GetReviewsByBoatIdAsync(2);
        result.Should().BeEmpty();
    }

    // ─── GetReviewByIdAsync ───

    [Fact]
    public async Task GetReviewByIdAsync_Existing_ReturnsDto()
    {
        var result = await _sut.GetReviewByIdAsync(1);
        result.Should().NotBeNull();
        result!.Rating.Should().Be(5);
        result.Comment.Should().Be("Excellent bateau !");
    }

    [Fact]
    public async Task GetReviewByIdAsync_NonExisting_ReturnsNull()
    {
        var result = await _sut.GetReviewByIdAsync(999);
        result.Should().BeNull();
    }

    // ─── CreateReviewAsync ───

    [Fact]
    public async Task CreateReviewAsync_Valid_CreatesAndUpdatesBoatRating()
    {
        var dto = new CreateReviewDto
        {
            BoatId = 2,
            UserId = _seed.Renter.Id,
            Rating = 4,
            Comment = "Très bien aussi"
        };

        var result = await _sut.CreateReviewAsync(dto);

        result.Should().NotBeNull();
        result.Rating.Should().Be(4);
        result.BoatId.Should().Be(2);

        // Verify boat rating was updated
        var boat = await _db.Boats.IgnoreQueryFilters().FirstAsync(b => b.Id == 2);
        boat.Rating.Should().Be(4m);
        boat.ReviewCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateReviewAsync_NonExistingBoat_ThrowsKeyNotFound()
    {
        var dto = new CreateReviewDto { BoatId = 999, UserId = _seed.Renter.Id, Rating = 5 };
        var act = () => _sut.CreateReviewAsync(dto);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateReviewAsync_NonExistingUser_ThrowsKeyNotFound()
    {
        var dto = new CreateReviewDto { BoatId = 1, UserId = Guid.NewGuid(), Rating = 5 };
        var act = () => _sut.CreateReviewAsync(dto);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── DeleteReviewAsync ───

    [Fact]
    public async Task DeleteReviewAsync_Existing_DeletesAndRecalcsRating()
    {
        var ok = await _sut.DeleteReviewAsync(1);
        ok.Should().BeTrue();

        // review2 remains with rating 4 → boat average should be 4
        var boat = await _db.Boats.IgnoreQueryFilters().FirstAsync(b => b.Id == 1);
        boat.ReviewCount.Should().Be(1);
        boat.Rating.Should().Be(4m);
    }

    [Fact]
    public async Task DeleteReviewAsync_NonExisting_ReturnsFalse()
    {
        var ok = await _sut.DeleteReviewAsync(999);
        ok.Should().BeFalse();
    }

    // ─── GetAverageRatingAsync ───

    [Fact]
    public async Task GetAverageRating_Boat1_Returns4Point5()
    {
        var avg = await _sut.GetAverageRatingAsync(1);
        avg.Should().Be(4.5);
    }

    [Fact]
    public async Task GetAverageRating_NoReviews_ReturnsZero()
    {
        var avg = await _sut.GetAverageRatingAsync(2);
        avg.Should().Be(0);
    }

    // ─── GetRecentReviewsAsync ───

    [Fact]
    public async Task GetRecentReviewsAsync_DefaultLimit_ReturnsOrdered()
    {
        var result = await _sut.GetRecentReviewsAsync();
        result.Should().HaveCount(2);
        // Most recent first
        result[0].Id.Should().Be(1); // review1 is more recent
    }

    [Fact]
    public async Task GetRecentReviewsAsync_Limit1_ReturnsOne()
    {
        var result = await _sut.GetRecentReviewsAsync(1);
        result.Should().HaveCount(1);
    }
}
