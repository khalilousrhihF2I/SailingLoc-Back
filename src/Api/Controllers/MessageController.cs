using Core.Entities;
using Core.Interfaces.Notifications;
using Core.Models.Templates;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/messages")]
    public class MessageController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;

        public MessageController(ApplicationDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        // Contact page - anonymous - send mail to admins (not saved)
        [HttpPost("contact")]
        [AllowAnonymous]
        public async Task<IActionResult> Contact([FromBody] ContactDto dto, CancellationToken ct)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Subject) ||
                string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { message = "Certains champs obligatoires sont manquants." });
            }

            // Load admin emails
            var admins = await _db.Users
                .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_db.Roles, j => j.ur.RoleId, r => r.Id, (j, r) => new { j.u, Role = r })
                .Where(x => x.Role.Name == "Admin")
                .Select(x => x.u.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToListAsync(ct);

            if (!admins.Any())
            {
                return Ok(new { message = "Votre message a été reçu, mais aucun administrateur n’est configuré pour le moment." });
            }

            // Build template model
            var model = new ContactMessageTemplateModel
            {
                BrandName = "SailingLoc",
                Name = dto.Name,
                Email = dto.Email,
                Topic = dto.Subject,
                Message = dto.Message,
                SentAt = DateTime.UtcNow
            };

            try
            {
                await _email.SendContactMessageEmailAsync(admins, model, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'envoi du message de contact : " + ex.Message);
                return StatusCode(500, new { message = "Une erreur est survenue lors de l’envoi du message. Veuillez réessayer plus tard." });
            }

            return Ok(new { message = "Votre message a été envoyé avec succès. Nous vous répondrons dès que possible." });
        }



        // Renter -> Owner: save message + send email to owner + admins
        [HttpPost("renter/to-owner")]
        [Authorize(Roles = "Renter")]
        public async Task<IActionResult> RenterToOwner([FromBody] SendMessageDto dto, CancellationToken ct)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (senderId == null) return Unauthorized();
            var sender = await _db.Users.FindAsync(Guid.Parse(senderId));
            var owner = await _db.Users.FindAsync(dto.ReceiverId);
            if (owner == null) return NotFound(new { message = "Owner not found" });

            var message = new Message
            {
                SenderId = Guid.Parse(senderId),
                ReceiverId = dto.ReceiverId,
                BoatId = dto.BoatId,
                BookingId = dto.BookingId,
                Subject = dto.Subject,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync(ct);

            // send email to owner + admins
            var admins = await _db.Users
                .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_db.Roles, j => j.ur.RoleId, r => r.Id, (j, r) => new { j.u, Role = r })
                .Where(x => x.Role.Name == "Admin")
                .Select(x => x.u.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToListAsync(ct);

            var recipients = new List<string>();
            if (!string.IsNullOrWhiteSpace(owner.Email)) recipients.Add(owner.Email);
            recipients.AddRange(admins);

            var model = new MessageTemplateModel
            {
                BrandName = "SailingLoc",
                SenderName = sender.FirstName + " " + sender.LastName,
                SenderEmail = sender.Email,
                ReceiverName = owner.FirstName + " " + owner.LastName,
                ReceiverEmail = owner.Email,
                Subject = dto.Subject,
                Content = dto.Content,
                BoatId = dto.BoatId,
                BookingId = dto.BookingId,
                CreatedAt = DateTime.UtcNow
            };

            try
            { 
                await _email.SendReservationCreatedEmailAsync(recipients, new ReservationTemplateModel());
            }
            catch { }

            return Ok(new { message = "Message sent" });
        }

        // Owner -> Renter: save message + send email to renter + admins
        [HttpPost("owner/to-renter")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> OwnerToRenter([FromBody] SendMessageDto dto, CancellationToken ct)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (senderId == null) return Unauthorized();
            var sender = await _db.Users.FindAsync(Guid.Parse(senderId));
            var renter = await _db.Users.FindAsync(dto.ReceiverId);
            if (renter == null) return NotFound(new { message = "Renter not found" });

            var message = new Message
            {
                SenderId = Guid.Parse(senderId),
                ReceiverId = dto.ReceiverId,
                BoatId = dto.BoatId,
                BookingId = dto.BookingId,
                Subject = dto.Subject,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync(ct);

            var admins = await _db.Users
                .Join(_db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_db.Roles, j => j.ur.RoleId, r => r.Id, (j, r) => new { j.u, Role = r })
                .Where(x => x.Role.Name == "Admin")
                .Select(x => x.u.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToListAsync(ct);

            var recipients = new List<string>();
            if (!string.IsNullOrWhiteSpace(renter.Email)) recipients.Add(renter.Email);
            recipients.AddRange(admins);

            var model = new MessageTemplateModel
            {
                BrandName = "SailingLoc",
                SenderName = sender.FirstName + " " + sender.LastName,
                SenderEmail = sender.Email,
                ReceiverName = renter.FirstName + " " + renter.LastName,
                ReceiverEmail = renter.Email,
                Subject = dto.Subject,
                Content = dto.Content,
                BoatId = dto.BoatId,
                BookingId = dto.BookingId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _email.SendReservationCreatedEmailAsync(recipients, new ReservationTemplateModel());
            }
            catch { }

            return Ok(new { message = "Message sent" });
        }

        // Get messages for a booking involving a specific user (sender or receiver)
        [HttpGet("booking/{bookingId}/user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetMessagesByBookingAndUser(string bookingId, Guid userId, CancellationToken ct)
        {
            // allow only the user themselves or admins to fetch
            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentId == null) return Unauthorized();

            if (!User.IsInRole("Admin") && Guid.Parse(currentId) != userId)
                return Forbid();

            var list = await _db.Messages
                .Where(m => m.BookingId == bookingId && (m.SenderId == userId || m.ReceiverId == userId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Subject,
                    m.Content,
                    m.IsRead,
                    m.ReadAt,
                    m.BookingId,
                    m.BoatId,
                    m.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(list);
        }
    }

    // DTOs
    public class ContactDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class SendMessageDto
    {
        public Guid ReceiverId { get; set; }
        public string? BookingId { get; set; }
        public int? BoatId { get; set; }
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
