using Api.DTOs;
using Core.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _um;
    private readonly RoleManager<AppRole> _rm;

    public AdminUsersController(ApplicationDbContext db, UserManager<AppUser> um, RoleManager<AppRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }

    /// <summary>Get paginated list of users.</summary>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Number of items per page (default 20)</param>
    /// <param name="q">Optional search query</param>
    /// <returns>Paginated list of users</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="401">Unauthorized: JWT missing or invalid</response>
    /// <response code="403">Forbidden: User not in Admin role</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
    {
        var query = _um.Users.AsQueryable();
        query = query.Where(u => u.UserType.Equals("admin") ||  u.UserType.Equals("Admin"));
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(u => (u.Email ?? "").Contains(q) || u.FirstName.Contains(q) || u.LastName.Contains(q));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    /// <summary>Get user by ID.</summary>
    /// <param name="id">User GUID</param>
    /// <returns>User object</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{id:guid}")]
    [ProducesResponseType(typeof(AppUser), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var u = await _um.Users.FirstOrDefaultAsync(x => x.Id == id);
        return u is null ? NotFound() : Ok(u);
    }

    /// <summary>Update user profile.</summary>
    /// <param name="id">User GUID</param>
    /// <param name="dto">Profile update data</param>
    /// <returns>Updated user</returns>
    /// <response code="200">Returns updated user</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("users/{id:guid}")]
    [ProducesResponseType(typeof(AppUser), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfileDto dto)
    {
        var u = await _um.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        u.FirstName = dto.FirstName;
        u.LastName = dto.LastName;
        u.Address.Street = dto.Address.Street;
        u.Address.City = dto.Address.City;
        u.Address.State = dto.Address.State;
        u.Address.PostalCode = dto.Address.PostalCode;
        u.Address.Country = dto.Address.Country;
        await _um.UpdateAsync(u);
        return Ok(u);
    }

    /// <summary>Create a new user.</summary>
    /// <param name="dto">User data</param>
    /// <returns>Created user</returns>
    /// <response code="201">Returns the created user</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPost("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var roleToAssign = "Admin";

        // ensure role exists
        if (!await _rm.RoleExistsAsync(roleToAssign))
            await _rm.CreateAsync(new AppRole { Name = roleToAssign });

        // prepare address (front may not send it for admin creation)
        var address = new Address
        {
            Street = dto.Address?.Street ?? string.Empty,
            City = dto.Address?.City ?? string.Empty,
            State = dto.Address?.State ?? string.Empty,
            PostalCode = dto.Address?.PostalCode ?? string.Empty,
            Country = dto.Address?.Country ?? string.Empty
        };

        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            BirthDate = dto.BirthDate,
            EmailConfirmed = true,
            UserType = roleToAssign.ToLower(),
            Address = address
        };

        var create = await _um.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
            return BadRequest(create.Errors);

        await _um.AddToRoleAsync(user, roleToAssign);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Delete a user.</summary>
    /// <param name="id">User GUID</param>
    /// <response code="204">User deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("users/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var u = await _um.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        await _um.DeleteAsync(u);
        return NoContent();
    }

    /// <summary>Assign roles to a user.</summary>
    /// <param name="id">User GUID</param>
    /// <param name="dto">Roles to assign</param>
    /// <returns>Updated roles of the user</returns>
    /// <response code="200">Roles assigned successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("users/{id:guid}/roles")]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignRolesDto dto)
    {
        var u = await _um.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        foreach (var r in dto.Roles.Distinct())
            if (!await _rm.RoleExistsAsync(r))
                await _rm.CreateAsync(new AppRole { Name = r });
        var existing = await _um.GetRolesAsync(u);
        var toAdd = dto.Roles.Except(existing).ToList();
        var toRemove = existing.Except(dto.Roles).ToList();
        if (toRemove.Any()) await _um.RemoveFromRolesAsync(u, toRemove);
        if (toAdd.Any()) await _um.AddToRolesAsync(u, toAdd);
        return Ok(await _um.GetRolesAsync(u));
    }

    /// <summary>Get audit logs (paginated).</summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <response code="200">Returns paginated logs</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Logs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var total = await _db.AuditLogs.CountAsync();
        var items = await _db.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    /// <summary>Dump user claims for debugging.</summary>
    /// <response code="200">Returns claims</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("debug/claims")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    [ProducesResponseType(401)]
    public IActionResult ClaimsDump() =>
        Ok(User.Claims.Select(c => new { c.Type, c.Value }));

    /// <summary>Check if the user is in a specific role.</summary>
    /// <param name="role">Role name</param>
    /// <response code="200">Returns boolean indicating role membership</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("debug/inrole")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public IActionResult InRole([FromQuery] string role = "Admin") =>
        Ok(new { role, isInRole = User.IsInRole(role) });
}
