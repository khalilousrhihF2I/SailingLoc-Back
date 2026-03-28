using Api.Controllers;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Controllers;

/// <summary>
/// Tests unitaires du BookingsController.
/// </summary>
public class BookingsControllerTests
{
    private readonly Mock<IBookingService> _serviceMock;
    private readonly BookingsController _sut;

    public BookingsControllerTests()
    {
        _serviceMock = new Mock<IBookingService>();
        // Controller needs ApplicationDbContext for invoice, but we only test non-invoice endpoints here
        var db = TestDbContextFactory.Create();
        _sut = new BookingsController(_serviceMock.Object, db);
    }

    // ─── GetAll ───

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetBookingsAsync(It.IsAny<BookingFilters>()))
            .ReturnsAsync(new List<BookingDto>());

        var result = await _sut.GetAll(new BookingFilters(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── GetById ───

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetBookingByIdAsync("BK1")).ReturnsAsync(new BookingDto { Id = "BK1" });

        var result = await _sut.GetById("BK1", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetBookingByIdAsync("NONE")).ReturnsAsync((BookingDto?)null);

        var result = await _sut.GetById("NONE", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── Create ───

    [Fact]
    public async Task Create_Valid_ReturnsCreatedAtAction()
    {
        var dto = new CreateBookingDto
        {
            BoatId = 1,
            RenterId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(15),
            DailyPrice = 200,
            ServiceFee = 50,
            RenterName = "Test User",
            RenterEmail = "test@test.com"
        };
        _serviceMock.Setup(s => s.CreateBookingAsync(dto))
            .ReturnsAsync(new BookingDto { Id = "BK-NEW", BoatId = 1 });

        var result = await _sut.Create(dto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_BoatNotFound_ReturnsNotFound()
    {
        var dto = new CreateBookingDto { BoatId = 999 };
        _serviceMock.Setup(s => s.CreateBookingAsync(dto))
            .ThrowsAsync(new KeyNotFoundException("Boat not found"));

        var result = await _sut.Create(dto, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_Invalid_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("BoatId", "Required");

        var result = await _sut.Create(new CreateBookingDto(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_Overlap_ReturnsBadRequest()
    {
        var dto = new CreateBookingDto { BoatId = 1 };
        _serviceMock.Setup(s => s.CreateBookingAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Overlap"));

        var result = await _sut.Create(dto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── Update ───

    [Fact]
    public async Task Update_Valid_ReturnsOk()
    {
        var dto = new UpdateBookingDto { Id = "BK1", Status = "confirmed" };
        _serviceMock.Setup(s => s.UpdateBookingAsync("BK1", dto))
            .ReturnsAsync(new BookingDto { Id = "BK1", Status = "confirmed" });

        var result = await _sut.Update("BK1", dto, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var dto = new UpdateBookingDto { Id = "NONE", Status = "confirmed" };
        _serviceMock.Setup(s => s.UpdateBookingAsync("NONE", dto))
            .ThrowsAsync(new KeyNotFoundException());

        var result = await _sut.Update("NONE", dto, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── Cancel ───

    [Fact]
    public async Task Cancel_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.CancelBookingAsync("BK1")).ReturnsAsync(true);

        var result = await _sut.Cancel("BK1", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Cancel_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.CancelBookingAsync("NONE")).ReturnsAsync(false);

        var result = await _sut.Cancel("NONE", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── GetByRenter ───

    [Fact]
    public async Task GetByRenter_ReturnsOk()
    {
        var renterId = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetBookingsByRenterAsync(renterId))
            .ReturnsAsync(new List<BookingDto>());

        var result = await _sut.GetByRenter(renterId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── GetByOwner ───

    [Fact]
    public async Task GetByOwner_ReturnsOk()
    {
        var ownerId = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetBookingsByOwnerAsync(ownerId))
            .ReturnsAsync(new List<BookingDto>());

        var result = await _sut.GetByOwner(ownerId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }
}
