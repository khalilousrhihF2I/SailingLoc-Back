using System.Threading.Tasks;
using Core.DTOs.Dashboard;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/v1/admin/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _service.GetStatsAsync();
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _service.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("boats")]
        public async Task<IActionResult> GetBoats()
        {
            var boats = await _service.GetBoatsAsync();
            return Ok(boats);
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _service.GetBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            var activity = await _service.GetActivityAsync();
            return Ok(activity);
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _service.GetPaymentStatsAsync();
            return Ok(payments);
        }
    }
}
