using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Notifications;
using Core.Models.Templates;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/user-documents")]
    public class UserDocumentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _files;
        private readonly IEmailService _email;

        public UserDocumentsController(ApplicationDbContext db, IFileStorageService files, IEmailService email)
        {
            _db = db;
            _files = files;
            _email = email;
        }

        // Upload a document (user uploads their own documents)
        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentForm form, CancellationToken ct)
        {
            if (form.File is null) return BadRequest(new { message = "File is required" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (userIdClaim == null) return Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            await using var stream = form.File.OpenReadStream();
            // reuse SaveAvatarAsync for simple storage path
            //var url = await _files.SaveAvatarAsync(stream, form.File.FileName, form.File.ContentType, ct);

            var doc = new UserDocument
            {
                UserId = userId,
                BoatId = form.BoatId,
                DocumentType = form.DocumentType ?? string.Empty,
                DocumentUrl = "NonImplemente/Aucun-service-de-stockage-V2-commingsooon.pdf",
                FileName = form.File.FileName,
                FileSize = form.File.Length,
                IsVerified = false,
                VerifiedAt = null,
                VerifiedBy = null,
                UploadedAt = DateTime.UtcNow
            };

            _db.UserDocuments.Add(doc);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // Request was cancelled (client closed connection / timeout).
                // Try to persist without the request cancellation token to avoid losing the uploaded file reference.
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Last-resort: log and rethrow so caller sees failure
                    Console.WriteLine($"Failed to save UserDocument after cancellation: {ex.Message}");
                    throw;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            // Notify admins about uploaded document (non-blocking)
            try
            {
                var admins = await _db.Users
                    .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                    .Join(_db.Roles, j => j.ur.RoleId, r => r.Id, (j, r) => new { j.u, Role = r })
                    .Where(x => x.Role.Name == "Admin")
                    .Select(x => x.u.Email)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToListAsync(ct);

                if (admins.Any())
                {
                    var user = await _db.Users.FindAsync(userId);
                    var model = new DocumentUploadedTemplateModel
                    {
                        BrandName = "SailingLoc",
                        UserName = user is not null ? $"{user.FirstName} {user.LastName}".Trim() : string.Empty,
                        UserId = userId,
                        DocumentType = doc.DocumentType,
                        Comment = null,
                        UploadedAt = doc.UploadedAt
                    };

                    // fire-and-forget email but await to observe failures
                    try { await _email.SendDocumentUploadedEmailAsync(admins, model, ct); } catch { }
                }
            }
            catch { }

            return CreatedAtAction(nameof(GetById), new { id = doc.Id }, doc);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (userIdClaim == null) return Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            var list = await _db.UserDocuments.Where(d => d.UserId == userId).OrderByDescending(d => d.UploadedAt).ToListAsync(ct);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var doc = await _db.UserDocuments.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (doc == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (userIdClaim == null) return Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            // owner or admin can view
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && doc.UserId != userId) return Forbid();

            return Ok(doc);
        }

        // Admin: list documents for a user
        [HttpGet("user/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
        {
            var list = await _db.UserDocuments.Where(d => d.UserId == userId).OrderByDescending(d => d.UploadedAt).ToListAsync(ct);
            return Ok(list);
        }

        // Admin: verify/unverify a document
        [HttpPatch("{id:int}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyDocument(int id, [FromQuery] bool verify, CancellationToken ct)
        {
            var doc = await _db.UserDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (doc == null) return NotFound();

            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (adminIdClaim == null) return Unauthorized();
            var adminId = Guid.Parse(adminIdClaim);

            doc.IsVerified = verify;
            doc.VerifiedAt = verify ? DateTime.UtcNow : null;
            doc.VerifiedBy = adminId;

            await _db.SaveChangesAsync(ct);

            return Ok();
        }
    }

    public class UploadDocumentForm
    {
        public IFormFile? File { get; set; }
        public string? DocumentType { get; set; }
        public int? BoatId { get; set; }
    }
}
