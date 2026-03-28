using Api.Controllers;
using Core.Entities;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.UnitTests.Controllers;

/// <summary>
/// Tests unitaires du DestinationsController.
/// </summary>
public class DestinationsControllerTests
{
    private readonly Mock<IDestinationService> _serviceMock;
    private readonly DestinationsController _sut;

    public DestinationsControllerTests()
    {
        _serviceMock = new Mock<IDestinationService>();
        _sut = new DestinationsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Destination>());
        var result = await _sut.GetAll(CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Destination { Id = 1, Name = "Test" });
        var result = await _sut.GetById(1, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Destination?)null);
        var result = await _sut.GetById(999, CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Search_ReturnsOk()
    {
        _serviceMock.Setup(s => s.SearchAsync("test")).ReturnsAsync(new List<Destination>());
        var result = await _sut.Search("test", CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByRegion_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByRegionAsync("Med")).ReturnsAsync(new List<Destination>());
        var result = await _sut.GetByRegion("Med", CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPopular_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetPopularAsync(4)).ReturnsAsync(new List<Destination>());
        var result = await _sut.GetPopular(4, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreatedAtAction()
    {
        var dest = new Destination { Name = "New", Region = "R", Country = "C" };
        _serviceMock.Setup(s => s.CreateAsync(dest)).ReturnsAsync(new Destination { Id = 10, Name = "New" });
        var result = await _sut.Create(dest, CancellationToken.None);
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Name", "Required");
        var result = await _sut.Create(new Destination(), CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_Valid_ReturnsOk()
    {
        var dest = new Destination { Name = "Updated", Region = "R", Country = "C" };
        _serviceMock.Setup(s => s.UpdateAsync(1, dest)).ReturnsAsync(new Destination { Id = 1, Name = "Updated" });
        var result = await _sut.Update(1, dest, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var dest = new Destination { Name = "X" };
        _serviceMock.Setup(s => s.UpdateAsync(999, dest)).ThrowsAsync(new KeyNotFoundException());
        var result = await _sut.Update(999, dest, CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

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
}
