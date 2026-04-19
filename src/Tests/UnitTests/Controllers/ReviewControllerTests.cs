using Api.Controllers;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Controllers;

/// <summary>
/// Tests unitaires du ReviewController.
/// </summary>
public class ReviewControllerTests
{
    private readonly Mock<IReviewService> _serviceMock;
    private readonly Mock<IAuditService> _auditMock;
    private readonly ReviewController _sut;

    public ReviewControllerTests()
    {
        _serviceMock = new Mock<IReviewService>();
        _auditMock = new Mock<IAuditService>();
        var db = TestDbContextFactory.Create();
        _sut = new ReviewController(_serviceMock.Object, db, _auditMock.Object);
        FakeUserHelper.SetFakeUser(_sut);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllReviewsAsync()).ReturnsAsync(new List<ReviewDto>());
        var result = await _sut.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetReviewByIdAsync(1)).ReturnsAsync(new ReviewDto { Id = 1 });
        var result = await _sut.GetById(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetReviewByIdAsync(999)).ReturnsAsync((ReviewDto?)null);
        var result = await _sut.GetById(999);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByBoat_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetReviewsByBoatIdAsync(1)).ReturnsAsync(new List<ReviewDto>());
        var result = await _sut.GetByBoat(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAverage_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAverageRatingAsync(1)).ReturnsAsync(4.5);
        var result = await _sut.GetAverage(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecent_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetRecentReviewsAsync(10)).ReturnsAsync(new List<ReviewDto>());
        var result = await _sut.GetRecent(10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreatedAtAction()
    {
        var dto = new CreateReviewDto { BoatId = 1, UserId = Guid.NewGuid(), Rating = 5 };
        _serviceMock.Setup(s => s.CreateReviewAsync(dto)).ReturnsAsync(new ReviewDto { Id = 10, BoatId = 1 });

        var result = await _sut.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_BoatNotFound_ReturnsNotFound()
    {
        var dto = new CreateReviewDto { BoatId = 999, UserId = Guid.NewGuid(), Rating = 5 };
        _serviceMock.Setup(s => s.CreateReviewAsync(dto)).ThrowsAsync(new KeyNotFoundException("Boat not found"));

        var result = await _sut.Create(dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Rating", "Required");
        var result = await _sut.Create(new CreateReviewDto());
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteReviewAsync(1)).ReturnsAsync(true);
        var result = await _sut.Delete(1);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteReviewAsync(999)).ReturnsAsync(false);
        var result = await _sut.Delete(999);
        result.Should().BeOfType<NotFoundResult>();
    }
}
