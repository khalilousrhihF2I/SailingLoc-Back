using Api.Controllers;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.UnitTests.Controllers;

/// <summary>
/// Tests unitaires du BoatsController — vérifie le comportement HTTP correct.
/// </summary>
public class BoatsControllerTests
{
    private readonly Mock<IBoatService> _serviceMock;
    private readonly BoatsController _sut;

    public BoatsControllerTests()
    {
        _serviceMock = new Mock<IBoatService>();
        _sut = new BoatsController(_serviceMock.Object);
    }

    // ─── GetAll ───

    [Fact]
    public async Task GetAll_ReturnsOkWithPaginatedBoats()
    {
        var paginatedResult = new PaginatedResult<BoatDto>
        {
            Items = new List<BoatDto> { new() { Id = 1, Name = "Test" } },
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        _serviceMock.Setup(s => s.GetBoatsAsync(It.IsAny<BoatFilters>())).ReturnsAsync(paginatedResult);

        var result = await _sut.GetAll(new BoatFilters(), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(paginatedResult);
    }

    // ─── GetById ───

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new BoatDto { Id = 1, Name = "Test" });

        var result = await _sut.GetById(1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((BoatDto?)null);

        var result = await _sut.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── Create ───

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var dto = new CreateBoatDto { Name = "New", Type = "Voilier", Location = "Nice", City = "Nice", Country = "FR", OwnerId = Guid.NewGuid() };
        var created = new BoatDto { Id = 10, Name = "New" };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _sut.Create(dto, CancellationToken.None);

        var cr = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        cr.ActionName.Should().Be(nameof(BoatsController.GetById));
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Name", "Required");

        var result = await _sut.Create(new CreateBoatDto(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── Update ───

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var dto = new UpdateBoatDto { Id = 2 };
        var result = await _sut.Update(1, dto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var dto = new UpdateBoatDto { Id = 999, Name = "X", Type = "Y", Location = "Z", City = "Z", Country = "Z", OwnerId = Guid.NewGuid() };
        _serviceMock.Setup(s => s.UpdateAsync(999, dto)).ThrowsAsync(new KeyNotFoundException());

        var result = await _sut.Update(999, dto, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_Valid_ReturnsOk()
    {
        var dto = new UpdateBoatDto { Id = 1, Name = "Updated", Type = "V", Location = "N", City = "N", Country = "FR", OwnerId = Guid.NewGuid() };
        _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(new BoatDto { Id = 1, Name = "Updated" });

        var result = await _sut.Update(1, dto, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── Delete ───

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _sut.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _sut.Delete(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── SetActive ───

    [Fact]
    public async Task SetActive_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.SetActiveAsync(1, false)).ReturnsAsync(true);

        var result = await _sut.SetActive(1, false, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetActive_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.SetActiveAsync(999, true)).ReturnsAsync(false);

        var result = await _sut.SetActive(999, true, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── SetVerified ───

    [Fact]
    public async Task SetVerified_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.SetVerifiedAsync(1, true)).ReturnsAsync(true);

        var result = await _sut.SetVerified(1, true, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetVerified_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.SetVerifiedAsync(999, true)).ReturnsAsync(false);

        var result = await _sut.SetVerified(999, true, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── GetByOwner ───

    [Fact]
    public async Task GetByOwner_ReturnsOk()
    {
        var ownerId = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByOwnerAsync(ownerId)).ReturnsAsync(new List<BoatDto>());

        var result = await _sut.GetByOwner(ownerId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── GetBySlug ───

    [Fact]
    public async Task GetBySlug_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetBySlugAsync("voilier-azur-1")).ReturnsAsync(new BoatDto { Id = 1, Name = "Voilier Azur", Slug = "voilier-azur-1" });

        var result = await _sut.GetBySlug("voilier-azur-1", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetBySlug_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetBySlugAsync("does-not-exist")).ReturnsAsync((BoatDto?)null);

        var result = await _sut.GetBySlug("does-not-exist", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
