using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors("Default")]
    public class BoatsController : ControllerBase
    {
        private readonly IBoatService _service;

        public BoatsController(IBoatService service)
        {
            _service = service;
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

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Create([FromBody] CreateBoatDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _service.CreateAsync(dto);
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