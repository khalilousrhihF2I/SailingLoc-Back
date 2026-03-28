using System;
using System.Threading.Tasks;
using Core.DTOs.RenterDashboard;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Renter")]
    [Route("api/v1/renter/dashboard")]
    public class RenterDashboardController : ControllerBase
    {
        private readonly IRenterDashboardService _service;

        public RenterDashboardController(IRenterDashboardService service)
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
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var stats = await _service.GetStatsAsync(userId.Value);
            return Ok(stats);
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var allList = await _service.GetBookingsAsync(userId.Value);
            var totalCount = allList.Count;
            var items = allList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize, TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0 });
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

        [HttpGet("documents")]
        public async Task<IActionResult> GetDocuments()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var docs = await _service.GetDocumentsAsync(userId.Value);
            return Ok(docs);
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var pm = await _service.GetPaymentMethodsAsync(userId.Value);
            return Ok(pm);
        }
    }
}
