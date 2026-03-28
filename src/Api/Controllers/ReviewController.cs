using System;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;
        private readonly ApplicationDbContext _db;

        public ReviewController(IReviewService service, ApplicationDbContext db)
        {
            _service = service;
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var res = await _service.GetAllReviewsAsync();
            return Ok(res);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _service.GetReviewByIdAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [HttpGet("boat/{boatId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByBoat(int boatId)
        {
            var res = await _service.GetReviewsByBoatIdAsync(boatId);
            return Ok(res);
        }

        [HttpGet("boat/{boatId:int}/average")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAverage(int boatId)
        {
            var avg = await _service.GetAverageRatingAsync(boatId);
            return Ok(new { average = avg });
        }

        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecent([FromQuery] int limit = 10)
        {
            var res = await _service.GetRecentReviewsAsync(limit);
            return Ok(res);
        }

        [HttpPost]
        [Authorize(Roles = "Renter,Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _service.CreateReviewAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            // Authorization for owner or admin must be enforced in service or here by checking user id
            var ok = await _service.DeleteReviewAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // ── Moderation endpoints ──

        /// <summary>Admin: list reviews pending moderation</summary>
        [HttpGet("moderation/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingReviews(CancellationToken ct)
        {
            var reviews = await _db.Reviews
                .Where(r => r.ModerationStatus == "pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
            return Ok(reviews);
        }

        /// <summary>Admin: approve a review</summary>
        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, CancellationToken ct)
        {
            var review = await _db.Reviews.FindAsync(new object[] { id }, ct);
            if (review == null) return NotFound();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            review.ModerationStatus = "approved";
            review.ModeratedBy = adminId != null ? Guid.Parse(adminId) : null;
            review.ModeratedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Ok(review);
        }

        /// <summary>Admin: reject a review with optional note</summary>
        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectReviewDto dto, CancellationToken ct)
        {
            var review = await _db.Reviews.FindAsync(new object[] { id }, ct);
            if (review == null) return NotFound();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            review.ModerationStatus = "rejected";
            review.ModerationNote = dto?.Reason;
            review.ModeratedBy = adminId != null ? Guid.Parse(adminId) : null;
            review.ModeratedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Ok(review);
        }
    }

    public class RejectReviewDto
    {
        public string? Reason { get; set; }
    }
}
