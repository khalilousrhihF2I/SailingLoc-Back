using System;

namespace Core.Models.Templates
{
    public class NewUserTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReservationTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string BookingId { get; set; } = string.Empty;
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public string RenterName { get; set; } = string.Empty;
        public Guid RenterId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
    public class ContactMessageTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string Topic { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime SentAt { get; set; }
    }
    public class CancellationRequestTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string BookingId { get; set; } = string.Empty;
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public Guid RequesterId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
    }

    public class DocumentUploadedTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string UserName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class ReservationApprovedTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string BookingId { get; set; } = string.Empty;
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public string RenterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class BoatApprovedTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public int BoatId { get; set; }
        public string BoatName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }

    public class MessageTemplateModel
    {
        public string BrandName { get; set; } = "SailingLoc";
        public string SenderName { get; set; } = string.Empty;
        public string? SenderEmail { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverEmail { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? BoatId { get; set; }
        public string? BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
