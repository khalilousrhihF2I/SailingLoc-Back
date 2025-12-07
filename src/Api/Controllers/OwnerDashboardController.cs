using Core.DTOs.OwnerDashboard;
using Core.DTOs.RenterDashboard;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v1/owner/dashboard")]
    public class OwnerDashboardController : ControllerBase
    {
        private readonly IOwnerDashboardService _service;

        public OwnerDashboardController(IOwnerDashboardService service)
        {
            _service = service;
        }

        private Guid? GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(id)) return null;
            return Guid.TryParse(id, out var g) ? g : (Guid?)null;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var ownerId = GetCurrentUserId();
            if (ownerId == null) return Unauthorized();
            var stats = await _service.GetStatsAsync(ownerId.Value);
            return Ok(stats);
        }

        [HttpGet("boats")]
        public async Task<IActionResult> GetBoats()
        {
            var ownerId = GetCurrentUserId();
            if (ownerId == null) return Unauthorized();
            var boats = await _service.GetBoatsAsync(ownerId.Value);
            return Ok(boats);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var p = await _service.GetProfileAsync(userId.Value);
            return Ok(p);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] RenterProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var updated = await _service.UpdateProfileAsync(userId.Value, dto);
            return Ok(updated);
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings()
        {
            var ownerId = GetCurrentUserId();
            if (ownerId == null) return Unauthorized();
            var bookings = await _service.GetBookingsAsync(ownerId.Value);
            return Ok(bookings);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var ownerId = GetCurrentUserId();
            if (ownerId == null) return Unauthorized();
            var rev = await _service.GetRevenueAsync(ownerId.Value);
            return Ok(rev);
        }

        [HttpGet("availability/{boatId:int}")]
        public async Task<IActionResult> GetAvailability(int boatId)
        {
            var ownerId = GetCurrentUserId();
            if (ownerId == null) return Unauthorized();
            try
            {
                var avail = await _service.GetAvailabilityAsync(boatId, ownerId.Value);
                return Ok(avail);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("availability/{boatId:int}")]
        public async Task<IActionResult> UpdateAvailability(int boatId, [FromBody] UpdateAvailabilityDto dto)
        {
            var ownerId = GetCurrentUserId();
            if (ownerId == null) return Unauthorized();
            try
            {
                var ok = await _service.UpdateAvailabilityAsync(boatId, ownerId.Value, dto);
                if (!ok) return BadRequest();
                return Ok(new { message = "Updated" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
