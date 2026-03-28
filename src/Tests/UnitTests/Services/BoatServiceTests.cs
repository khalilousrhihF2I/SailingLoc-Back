using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Services;

/// <summary>
/// Tests unitaires pour BoatService — couverture complète de toutes les méthodes.
/// </summary>
public class BoatServiceTests : IAsyncLifetime
{
    private Infrastructure.Data.ApplicationDbContext _db = null!;
    private BoatService _sut = null!;
    private TestSeedData _seed = null!;

    public async Task InitializeAsync()
    {
        _db = TestDbContextFactory.Create();
        _seed = await TestDbContextFactory.SeedStandardDataAsync(_db);
        var repo = new BoatRepository(_db);
        _sut = new BoatService(repo);
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    // ─── GetBoatsAsync ───

    [Fact]
    public async Task GetBoatsAsync_NoFilters_ReturnsOnlyActiveVerifiedNonDeleted()
    {
        var result = (await _sut.GetBoatsAsync(new BoatFilters())).Items.ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.IsActive && b.IsVerified && !b.IsDeleted);
    }

    [Fact]
    public async Task GetBoatsAsync_FilterByLocation_ReturnsMatching()
    {
        var result = (await _sut.GetBoatsAsync(new BoatFilters { Location = "Nice" })).Items.ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Voilier Azur");
    }

    [Fact]
    public async Task GetBoatsAsync_FilterByType_ReturnsMatching()
    {
        var result = (await _sut.GetBoatsAsync(new BoatFilters { Type = "Catamaran" })).Items.ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Catamaran Breizh");
    }

    [Fact]
    public async Task GetBoatsAsync_FilterByPriceRange_ReturnsMatching()
    {
        var result = (await _sut.GetBoatsAsync(new BoatFilters { PriceMin = 300, PriceMax = 500 })).Items.ToList();

        result.Should().HaveCount(1);
        result[0].Price.Should().BeGreaterOrEqualTo(300);
    }

    [Fact]
    public async Task GetBoatsAsync_FilterByCapacityMin_ReturnsMatching()
    {
        var result = (await _sut.GetBoatsAsync(new BoatFilters { CapacityMin = 7 })).Items.ToList();

        result.Should().HaveCount(1);
        result[0].Capacity.Should().BeGreaterOrEqualTo(7);
    }

    [Fact]
    public async Task GetBoatsAsync_FilterByDestinationId_ReturnsMatching()
    {
        var result = (await _sut.GetBoatsAsync(new BoatFilters { Destination = "2" })).Items.ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Catamaran Breizh");
    }

    [Fact]
    public async Task GetBoatsAsync_FilterByDestinationName_ReturnsMatching()
    {
        var result = (await _sut.GetBoatsAsync(new BoatFilters { Destination = "Saint-Malo" })).Items.ToList();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetBoatsAsync_ReturnsPaginationMetadata()
    {
        var result = await _sut.GetBoatsAsync(new BoatFilters { Page = 1, PageSize = 1 });

        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.TotalPages.Should().Be(2);
        result.Items.Count().Should().Be(1);
    }

    // ─── GetByIdAsync ───

    [Fact]
    public async Task GetByIdAsync_ExistingBoat_ReturnsDto()
    {
        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Voilier Azur");
        result.Owner.Should().NotBeNull();
        result.Images.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedBoat_ReturnsNull()
    {
        // boat3 is soft-deleted; EF query filter should exclude it
        var result = await _sut.GetByIdAsync(3);
        result.Should().BeNull();
    }

    // ─── CreateAsync ───

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedBoat()
    {
        var dto = new CreateBoatDto
        {
            Name = "New Boat",
            Type = "Voilier",
            Location = "Cannes",
            City = "Cannes",
            Country = "France",
            Price = 180,
            Capacity = 4,
            Cabins = 1,
            Length = 10,
            Year = 2023,
            OwnerId = _seed.Owner.Id,
            Equipment = new[] { "GPS", "VHF" },
            Images = new[] { "/img/new1.jpg" }
        };

        var result = await _sut.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Boat");
        result.IsActive.Should().BeTrue();
        result.IsVerified.Should().BeFalse();
        result.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
    {
        var act = () => _sut.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ─── UpdateAsync ───

    [Fact]
    public async Task UpdateAsync_ExistingBoat_ReturnsUpdated()
    {
        var dto = new UpdateBoatDto
        {
            Id = 1,
            Name = "Voilier Azur Updated",
            Type = "Voilier",
            Location = "Nice",
            City = "Nice",
            Country = "France",
            Price = 300,
            Capacity = 6,
            Cabins = 2,
            Length = 12.5m,
            Year = 2020,
            OwnerId = _seed.Owner.Id
        };

        var result = await _sut.UpdateAsync(1, dto);

        result.Name.Should().Be("Voilier Azur Updated");
        result.Price.Should().Be(300);
    }

    [Fact]
    public async Task UpdateAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        var dto = new UpdateBoatDto { Id = 999, Name = "X", Type = "Y", Location = "Z", City = "Z", Country = "Z", OwnerId = _seed.Owner.Id };
        var act = () => _sut.UpdateAsync(999, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
    {
        var act = () => _sut.UpdateAsync(1, null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ─── DeleteAsync (soft delete) ───

    [Fact]
    public async Task DeleteAsync_ExistingBoat_SoftDeletes()
    {
        var ok = await _sut.DeleteAsync(1);
        ok.Should().BeTrue();

        // Verify the boat is marked as deleted in database (bypass query filter)
        var boat = await _db.Boats.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == 1);
        boat!.IsDeleted.Should().BeTrue();
        boat.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ReturnsFalse()
    {
        var ok = await _sut.DeleteAsync(9999);
        ok.Should().BeFalse();
    }

    // ─── GetByOwnerAsync ───

    [Fact]
    public async Task GetByOwnerAsync_ReturnsOnlyOwnerBoats_ExcludingDeleted()
    {
        var result = (await _sut.GetByOwnerAsync(_seed.Owner.Id)).ToList();

        // Owner owns boat1, boat2, boat3(deleted). Should return 2.
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => !b.IsDeleted);
    }

    [Fact]
    public async Task GetByOwnerAsync_NonExistingOwner_ReturnsEmpty()
    {
        var result = (await _sut.GetByOwnerAsync(Guid.NewGuid())).ToList();
        result.Should().BeEmpty();
    }

    // ─── SetActiveAsync ───

    [Fact]
    public async Task SetActiveAsync_ExistingBoat_TogglesActive()
    {
        var ok = await _sut.SetActiveAsync(1, false);
        ok.Should().BeTrue();

        var boat = await _db.Boats.IgnoreQueryFilters().FirstAsync(b => b.Id == 1);
        boat.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SetActiveAsync_NonExisting_ReturnsFalse()
    {
        var ok = await _sut.SetActiveAsync(9999, true);
        ok.Should().BeFalse();
    }

    // ─── SetVerifiedAsync ───

    [Fact]
    public async Task SetVerifiedAsync_ExistingBoat_TogglesVerified()
    {
        var ok = await _sut.SetVerifiedAsync(2, false);
        ok.Should().BeTrue();

        var boat = await _db.Boats.IgnoreQueryFilters().FirstAsync(b => b.Id == 2);
        boat.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async Task SetVerifiedAsync_NonExisting_ReturnsFalse()
    {
        var ok = await _sut.SetVerifiedAsync(9999, true);
        ok.Should().BeFalse();
    }

    // ─── DTO Mapping ───

    [Fact]
    public async Task MapToDto_IncludesAllNavigations()
    {
        var result = await _sut.GetByIdAsync(1);
        result.Should().NotBeNull();
        result!.Owner.Should().NotBeNull();
        result.Images.Should().NotBeNull();
        result.Availabilities.Should().NotBeNull();
        result.Reviews.Should().NotBeNull();
    }

    // ─── GetBySlugAsync ───

    [Fact]
    public async Task GetBySlugAsync_NonExistingSlug_ReturnsNull()
    {
        var result = await _sut.GetBySlugAsync("does-not-exist");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_NullSlug_ReturnsNull()
    {
        var result = await _sut.GetBySlugAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_EmptySlug_ReturnsNull()
    {
        var result = await _sut.GetBySlugAsync("");
        result.Should().BeNull();
    }

    // ─── Create generates slug ───

    [Fact]
    public async Task CreateAsync_GeneratesSlug()
    {
        var dto = new CreateBoatDto
        {
            Name = "Mon Super Bateau",
            Type = "Voilier",
            Location = "Cannes",
            City = "Cannes",
            Country = "France",
            Price = 180,
            Capacity = 4,
            Cabins = 1,
            Length = 10,
            Year = 2023,
            OwnerId = _seed.Owner.Id
        };

        var result = await _sut.CreateAsync(dto);

        result.Slug.Should().NotBeNullOrEmpty();
        result.Slug.Should().Contain("mon-super-bateau");
    }
}
