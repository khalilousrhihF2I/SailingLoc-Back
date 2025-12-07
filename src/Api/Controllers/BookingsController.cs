using System;
using System.Threading;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Net.Http.Headers;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;
        private readonly ApplicationDbContext _db;

        public BookingsController(IBookingService service, ApplicationDbContext db)
        {
            _service = service;
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Owner,Renter")]
        public async Task<IActionResult> GetAll([FromQuery] BookingFilters filters, CancellationToken ct)
        {
            var list = await _service.GetBookingsAsync(filters);
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Owner,Renter")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var b = await _service.GetBookingByIdAsync(id);
            if (b == null) return NotFound();
            return Ok(b);
        }

        [HttpPost]
        [Authorize(Roles = "Renter,Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _service.CreateBookingAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(new { message = ioe.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateBookingDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updated = await _service.UpdateBookingAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Cancel(string id, CancellationToken ct)
        {
            var ok = await _service.CancelBookingAsync(id);
            if (!ok) return NotFound();
            return Ok(ok);
        }

        [HttpGet("renter/{renterId:guid}")]
        [Authorize(Roles = "Admin,Renter")]
        public async Task<IActionResult> GetByRenter(Guid renterId, CancellationToken ct)
        {
            var list = await _service.GetBookingsByRenterAsync(renterId);
            return Ok(list);
        }

        [HttpGet("owner/{ownerId:guid}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> GetByOwner(Guid ownerId, CancellationToken ct)
        {
            var list = await _service.GetBookingsByOwnerAsync(ownerId);
            return Ok(list);
        }

        [HttpGet("{id}/invoice")]
        [Authorize(Roles = "Admin,Owner,Renter")]
        public async Task<IActionResult> GetInvoice(string id, CancellationToken ct)
        {
            // fetch booking details (use db to get navigations)
            var booking = await _db.Bookings
                .Include(b => b.Boat).ThenInclude(x => x.Owner)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (booking == null) return NotFound();

            // authorization: only admin, the renter or the boat owner can download
            var current = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (current == null) return Unauthorized();
            var currentGuid = Guid.Parse(current);
            if (!User.IsInRole("Admin") && booking.RenterId != currentGuid && booking.Boat.OwnerId != currentGuid)
                return Forbid();

            // generate simple invoice PDF using QuestPDF
            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12).FontColor("#333"));

                    // ---------------- HEADER ----------------
                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SailingLoc")
                                    .FontSize(28).Bold().FontColor("#0A84FF");

                                col.Item().Text("Facture de réservation")
                                    .FontSize(12).FontColor("#555");
                            });

                            row.ConstantItem(150).AlignRight().Column(col =>
                            {
                                col.Item().Text($"Facture n°: {booking.Id}")
                                    .FontSize(12).FontColor("#777");
                                col.Item().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}")
                                    .FontSize(12).FontColor("#777");
                            });
                        });

                        header.Item().PaddingTop(10).BorderBottom(1).BorderColor("#DDD");
                    });

                    // ---------------- CONTENT ----------------
                    page.Content().Column(col =>
                    {
                        // ------ PARTIES ------
                        col.Item().PaddingVertical(20).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("Locataire")
                                    .SemiBold().FontSize(14).FontColor("#0A84FF");

                                left.Item().Text($"{booking.RenterName}")
                                    .FontSize(12);

                                left.Item().Text(booking.RenterEmail)
                                    .FontSize(11).FontColor("#777");

                                if (!string.IsNullOrWhiteSpace(booking.RenterPhone))
                                {
                                    left.Item().Text(booking.RenterPhone)
                                        .FontSize(11).FontColor("#777");
                                }
                            });

                            row.RelativeItem().Column(right =>
                            {
                                right.Item().Text("Propriétaire")
                                    .SemiBold().FontSize(14).FontColor("#0A84FF");

                                right.Item().Text($"{booking.Boat.Owner.FirstName} {booking.Boat.Owner.LastName}")
                                    .FontSize(12);

                                right.Item().Text($"Email : {booking.Boat.Owner.Email}")
                                    .FontSize(11).FontColor("#777");
                            });
                        });

                        // Divider
                        col.Item().PaddingVertical(10).BorderBottom(1).BorderColor("#EEE");


                        // ------ RESERVATION DETAILS ------
                        col.Item().PaddingTop(20).Column(section =>
                        {
                            section.Item().Text("Détails de la réservation")
                                .SemiBold().FontSize(16).FontColor("#0A84FF");

                            section.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Bateau : {booking.Boat.Name}");
                                    c.Item().Text($"Type : {booking.Boat.Type}");
                                    c.Item().Text($"Location : {booking.Boat.Location}");
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Début : {booking.StartDate:yyyy-MM-dd}");
                                    c.Item().Text($"Fin : {booking.EndDate:yyyy-MM-dd}");
                                    c.Item().Text($"Durée : {(booking.EndDate - booking.StartDate).Days} jours");
                                });
                            });
                        });

                        col.Item().PaddingVertical(20).BorderBottom(1).BorderColor("#EEE");

                        // ------ AMOUNTS TABLE ------
                        col.Item().PaddingTop(20).Column(section =>
                        {
                            section.Item().Text("Montant de la réservation")
                                .SemiBold().FontSize(16).FontColor("#0A84FF");

                            section.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn();
                                    cols.ConstantColumn(120);
                                });

                                // Header row
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Montant").SemiBold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.Padding(5).Background("#F7F9FC");
                                    }
                                });

                                // Subtotal
                                table.Cell().Element(NormalCell).Text($"Location ({booking.DailyPrice}€/jour)");
                                table.Cell().Element(NormalCell).AlignRight().Text(booking.Subtotal.ToString("C"));

                                // Service fee
                                table.Cell().Element(NormalCell).Text("Frais de service (10%)");
                                table.Cell().Element(NormalCell).AlignRight().Text(booking.ServiceFee.ToString("C"));

                                static IContainer NormalCell(IContainer container)
                                {
                                    return container.Padding(5).BorderBottom(1).BorderColor("#EEE");
                                }
                            });
                        });

                        // ------ TOTAL ------
                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(200).Column(c =>
                            {
                                c.Item().Background("#0A84FF").Padding(10).AlignCenter()
                                 .Text("Total TTC").FontSize(14).Bold().FontColor("#FFF");

                                c.Item().Padding(10).Border(1).BorderColor("#0A84FF")
                                 .AlignCenter()
                                 .Text(booking.TotalPrice.ToString("C")).FontSize(16).Bold().FontColor("#333");
                            });
                        });
                    });

                    // ---------------- FOOTER ----------------
                    page.Footer().AlignCenter().PaddingVertical(10).Column(footer =>
                    {
                        footer.Item().BorderTop(1).BorderColor("#DDD");
                        footer.Item().PaddingTop(8).Text("Merci d’avoir utilisé SailingLoc ❤️")
                            .FontSize(11).FontColor("#777");
                        footer.Item().Text("www.sailingloc.com")
                            .FontSize(10).FontColor("#0A84FF");
                    });
                });
            }).GeneratePdf();


            var filename = $"facture-{booking.Id}.pdf";
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("attachment") { FileNameStar = filename }.ToString();
            return File(bytes, "application/pdf", filename);
        }
    }
}
