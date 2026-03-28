using Api.Controllers;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.UnitTests.Controllers;

/// <summary>
/// Tests unitaires du AvailabilityController.
/// </summary>
public class AvailabilityControllerTests
{
    private readonly Mock<IAvailabilityService> _serviceMock;
    private readonly AvailabilityController _sut;

    public AvailabilityControllerTests()
    {
        _serviceMock = new Mock<IAvailabilityService>();
        _sut = new AvailabilityController(_serviceMock.Object);
    }

    // ─── Check ───

    [Fact]
    public async Task Check_ValidDates_ReturnsOk()
    {
        _serviceMock.Setup(s => s.CheckAvailabilityAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(new AvailabilityCheck { IsAvailable = true, Message = "Available" });

        var result = await _sut.Check(1, "2026-06-01", "2026-06-10", null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Check_InvalidDates_ReturnsBadRequest()
    {
        var result = await _sut.Check(1, "not-a-date", "also-invalid", null, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── GetUnavailable ───

    [Fact]
    public async Task GetUnavailable_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetUnavailableDatesAsync(1, null, null))
            .ReturnsAsync(new List<UnavailablePeriod>());

        var result = await _sut.GetUnavailable(1, null, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── GetUnavailableAlias ───

    [Fact]
    public async Task GetUnavailableAlias_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetUnavailableDatesAsync(1, null, null))
            .ReturnsAsync(new List<UnavailablePeriod>());

        var result = await _sut.GetUnavailableAlias(1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ─── AddUnavailablePeriod ───

    [Fact]
    public async Task AddUnavailablePeriod_Valid_ReturnsOk()
    {
        var dto = new AddUnavailablePeriodDto
        {
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(15),
            Reason = "Test"
        };
        _serviceMock.Setup(s => s.AddUnavailablePeriodAsync(1, dto))
            .ReturnsAsync(new UnavailablePeriod { Type = "blocked" });

        var result = await _sut.AddUnavailablePeriod(1, dto, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddUnavailablePeriod_InvalidModel_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("StartDate", "Required");
        var result = await _sut.AddUnavailablePeriod(1, new AddUnavailablePeriodDto(), CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── DeleteUnavailablePeriod ───

    [Fact]
    public async Task DeleteUnavailablePeriod_InvalidDate_ReturnsBadRequest()
    {
        var result = await _sut.DeleteUnavailablePeriod(1, "invalid-date", CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteUnavailablePeriod_Existing_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.RemoveUnavailablePeriodAsync(1, It.IsAny<DateTime>())).ReturnsAsync(true);
        var result = await _sut.DeleteUnavailablePeriod(1, "2026-06-01", CancellationToken.None);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteUnavailablePeriod_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.RemoveUnavailablePeriodAsync(1, It.IsAny<DateTime>())).ReturnsAsync(false);
        var result = await _sut.DeleteUnavailablePeriod(1, "2026-06-01", CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── Block ───

    [Fact]
    public async Task Block_Valid_ReturnsOk()
    {
        var dto = new CreateAvailabilityDto { BoatId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5) };
        _serviceMock.Setup(s => s.BlockPeriodAsync(dto)).ReturnsAsync(true);
        var result = await _sut.Block(dto, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Block_Invalid_ReturnsBadRequest()
    {
        var dto = new CreateAvailabilityDto();
        _serviceMock.Setup(s => s.BlockPeriodAsync(dto)).ReturnsAsync(false);
        var result = await _sut.Block(dto, CancellationToken.None);
        result.Should().BeOfType<BadRequestResult>();
    }

    // ─── Unblock ───

    [Fact]
    public async Task Unblock_Existing_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.UnblockPeriodAsync(1)).ReturnsAsync(true);
        var result = await _sut.Unblock(1, CancellationToken.None);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Unblock_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.UnblockPeriodAsync(999)).ReturnsAsync(false);
        var result = await _sut.Unblock(999, CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }
}
