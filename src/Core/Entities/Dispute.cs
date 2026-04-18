using System;

namespace Core.Entities
{
    /// <summary>
    /// Gestion des litiges entre locataires et propriétaires.
    /// </summary>
    public class Dispute
    {
        public int Id { get; set; }
        public string BookingId { get; set; } = "";
        public Guid ReporterId { get; set; }       // user who opened the dispute
        public Guid? RespondentId { get; set; }     // the other party

        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "open"; // open, under_review, resolved, closed
        public string? Resolution { get; set; }
        public string? AdminNote { get; set; }

        public Guid? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
        public AppUser Reporter { get; set; } = null!;
    }
}
