using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors("Default")]
    public class BoatsController : ControllerBase
    {
        private readonly IBoatService _service;
        private readonly IAuditService _audit;

        public BoatsController(IBoatService service, IAuditService audit)
        {
            _service = service;
            _audit = audit;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] BoatFilters filters, CancellationToken ct)
        {
            var boats = await _service.GetBoatsAsync(filters);
            return Ok(boats);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var boat = await _service.GetByIdAsync(id);
            if (boat == null) return NotFound();
            return Ok(boat);
        }

        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
        {
            var boat = await _service.GetBySlugAsync(slug);
            if (boat == null) return NotFound();
            return Ok(boat);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Create([FromBody] CreateBoatDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _service.CreateAsync(dto);

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _audit.LogAsync(Guid.TryParse(uid, out var g) ? g : null, "BOAT_CREATE", ip, $"Boat '{created.Name}' (ID: {created.Id}) created");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBoatDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest(new { message = "Id mismatch" });

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();

            var uid2 = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var ip2 = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _audit.LogAsync(Guid.TryParse(uid2, out var g2) ? g2 : null, "BOAT_DELETE", ip2, $"Boat {id} deleted");

            return NoContent();
        }

        // Patch isActive flag
        [HttpPatch("{id:int}/active")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool isActive, CancellationToken ct)
        {
            var ok = await _service.SetActiveAsync(id, isActive);
            if (!ok) return NotFound();
            return Ok();
        }

        // Patch verification flag - admin only
        [HttpPatch("{id:int}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetVerified(int id, [FromQuery] bool isVerified, CancellationToken ct)
        {
            var ok = await _service.SetVerifiedAsync(id, isVerified);
            if (!ok) return NotFound();

            var uid3 = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var ip3 = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _audit.LogAsync(Guid.TryParse(uid3, out var g3) ? g3 : null, "BOAT_VERIFY", ip3, $"Boat {id} verification set to {isVerified}");

            return Ok();
        }

        [HttpGet("owner/{ownerId:guid}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> GetByOwner(Guid ownerId, CancellationToken ct)
        {
            var boats = await _service.GetByOwnerAsync(ownerId);
            return Ok(boats);
        }
    }
}