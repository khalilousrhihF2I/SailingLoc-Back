using System;

namespace Core.DTOs.RenterDashboard
{
    public class RenterDocumentDto
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public bool Verified { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
