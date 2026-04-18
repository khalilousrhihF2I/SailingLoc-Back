using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DisputeController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DisputeController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Create a dispute for a booking</summary>
        [HttpPost]
        [Authorize(Roles = "Renter,Owner")]
        public async Task<IActionResult> Create([FromBody] CreateDisputeDto dto, CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var booking = await _db.Bookings.FindAsync(new object[] { dto.BookingId }, ct);
            if (booking == null) return NotFound(new { message = "Réservation introuvable" });

            var dispute = new Dispute
            {
                BookingId = dto.BookingId,
                ReporterId = userId.Value,
                RespondentId = dto.RespondentId,
                Subject = dto.Subject,
                Description = dto.Description,
                Status = "open",
                CreatedAt = DateTime.UtcNow
            };

            _db.Disputes.Add(dispute);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = dispute.Id }, dispute);
        }

        /// <summary>Get dispute by ID</summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var dispute = await _db.Disputes
                .Include(d => d.Reporter)
                .Include(d => d.Booking)
                .FirstOrDefaultAsync(d => d.Id == id, ct);

            if (dispute == null) return NotFound();

            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && dispute.ReporterId != userId && dispute.RespondentId != userId)
                return Forbid();

            return Ok(dispute);
        }

        /// <summary>List user's disputes</summary>
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var disputes = await _db.Disputes
                .Where(d => d.ReporterId == userId || d.RespondentId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(ct);

            return Ok(disputes);
        }

        /// <summary>Admin: list all disputes</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
        {
            var query = _db.Disputes
                .Include(d => d.Reporter)
                .Include(d => d.Booking)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(d => d.Status == status);

            var disputes = await query.OrderByDescending(d => d.CreatedAt).ToListAsync(ct);
            return Ok(disputes);
        }

        /// <summary>Admin: update dispute status and resolution</summary>
        [HttpPatch("{id:int}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(int id, [FromBody] ResolveDisputeDto dto, CancellationToken ct)
        {
            var dispute = await _db.Disputes.FindAsync(new object[] { id }, ct);
            if (dispute == null) return NotFound();

            var adminId = GetUserId();
            dispute.Status = dto.Status ?? "resolved";
            dispute.Resolution = dto.Resolution;
            dispute.AdminNote = dto.AdminNote;
            dispute.ResolvedBy = adminId;
            dispute.ResolvedAt = DateTime.UtcNow;
            dispute.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Ok(dispute);
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return claim != null ? Guid.Parse(claim) : null;
        }
    }

    public class CreateDisputeDto
    {
        public string BookingId { get; set; } = "";
        public Guid? RespondentId { get; set; }
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class ResolveDisputeDto
    {
        public string? Status { get; set; }
        public string? Resolution { get; set; }
        public string? AdminNote { get; set; }
    }
}
