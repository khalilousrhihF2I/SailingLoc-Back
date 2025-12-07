using System;
using System.Threading;
using System.Threading.Tasks;
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
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _service;

        public AvailabilityController(IAvailabilityService service)
        {
            _service = service;
        }

        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<IActionResult> Check([FromQuery] int boatId, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string? excludeBookingId, CancellationToken ct)
        {
            if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
                return BadRequest("Invalid dates");

            var res = await _service.CheckAvailabilityAsync(boatId, start, end, excludeBookingId);
            return Ok(res);
        }

        [HttpGet("unavailable")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUnavailable([FromQuery] int boatId, [FromQuery] string? startDate, [FromQuery] string? endDate, CancellationToken ct)
        {
            DateTime? start = null; DateTime? end = null;
            if (!string.IsNullOrWhiteSpace(startDate) && DateTime.TryParse(startDate, out var s)) start = s;
            if (!string.IsNullOrWhiteSpace(endDate) && DateTime.TryParse(endDate, out var e)) end = e;
            var res = await _service.GetUnavailableDatesAsync(boatId, start, end);
            return Ok(res);
        }

        [HttpGet("boats/{boatId}/unavailable")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUnavailableAlias(int boatId, CancellationToken ct)
        {
            var res = await _service.GetUnavailableDatesAsync(boatId, null, null);
            return Ok(res);
        }

        [HttpPost("boats/{boatId}/unavailable")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> AddUnavailablePeriod(int boatId, [FromBody] AddUnavailablePeriodDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var res = await _service.AddUnavailablePeriodAsync(boatId, dto);
            return Ok(res);
        }

        [HttpDelete("boats/{boatId}/unavailable/{startDate}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> DeleteUnavailablePeriod(int boatId, string startDate, CancellationToken ct)
        {
            if (!DateTime.TryParse(startDate, out var start)) return BadRequest("Invalid date");
            var ok = await _service.RemoveUnavailablePeriodAsync(boatId, start);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("block")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Block([FromBody] CreateAvailabilityDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ok = await _service.BlockPeriodAsync(dto);
            if (!ok) return BadRequest();
            return Ok(new { message = "Blocked" });
        }

        [HttpDelete("{availabilityId}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Unblock(int availabilityId, CancellationToken ct)
        {
            var ok = await _service.UnblockPeriodAsync(availabilityId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
