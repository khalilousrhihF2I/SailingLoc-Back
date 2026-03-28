using Api.Controllers;
using Core.DTOs;
using Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.UnitTests.Controllers;

/// <summary>
/// Tests unitaires du UserController.
/// </summary>
public class UserControllerTests
{
    private readonly Mock<IUserService> _serviceMock;
    private readonly UserController _sut;

    public UserControllerTests()
    {
        _serviceMock = new Mock<IUserService>();
        _sut = new UserController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetUsersAsync()).ReturnsAsync(new List<UserDto>());
        var result = await _sut.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(new UserDto { Id = id });
        var result = await _sut.GetById(id);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync((UserDto?)null);
        var result = await _sut.GetById(id);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByEmail_Existing_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetUserByEmailAsync("test@test.com")).ReturnsAsync(new UserDto());
        var result = await _sut.GetByEmail("test@test.com");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByEmail_NonExisting_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetUserByEmailAsync("none@test.com")).ReturnsAsync((UserDto?)null);
        var result = await _sut.GetByEmail("none@test.com");
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var dto = new CreateUserDto { FirstName = "A", LastName = "B", Email = "a@b.com", Password = "pass1234" };
        _serviceMock.Setup(s => s.CreateUserAsync(dto)).ReturnsAsync(new UserDto { Id = Guid.NewGuid() });
        var result = await _sut.Create(dto);
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Email", "Required");
        var result = await _sut.Create(new CreateUserDto());
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidOperation_ReturnsBadRequest()
    {
        var dto = new CreateUserDto { FirstName = "A", LastName = "B", Email = "a@b.com", Password = "pass1234" };
        _serviceMock.Setup(s => s.CreateUserAsync(dto)).ThrowsAsync(new InvalidOperationException("Duplicate"));
        var result = await _sut.Create(dto);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateUserDto { Id = Guid.NewGuid() };
        var result = await _sut.Update(id, dto);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_Valid_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateUserDto { Id = id, FirstName = "X" };
        _serviceMock.Setup(s => s.UpdateUserAsync(dto)).ReturnsAsync(new UserDto { Id = id });
        var result = await _sut.Update(id, dto);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateUserDto { Id = id };
        _serviceMock.Setup(s => s.UpdateUserAsync(dto)).ThrowsAsync(new KeyNotFoundException());
        var result = await _sut.Update(id, dto);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(true);
        var result = await _sut.Delete(id);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExisting_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(false);
        var result = await _sut.Delete(id);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Verify_Existing_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.VerifyUserAsync(id)).ReturnsAsync(new UserDto { Id = id, Verified = true });
        var result = await _sut.Verify(id);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Verify_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.VerifyUserAsync(id)).ThrowsAsync(new KeyNotFoundException());
        var result = await _sut.Verify(id);
        result.Should().BeOfType<NotFoundResult>();
    }
}
