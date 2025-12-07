using System;

namespace Core.Entities
{
    public class UserDocument
    {
        public int Id { get; set; }
        public Guid UserId { get; set; } 
        public int? BoatId { get; set; }
        public string DocumentType { get; set; } = "";
        public string DocumentUrl { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime UploadedAt { get; set; }

        public AppUser User { get; set; } = null!;
        public AppUser VerifiedByUser { get; set; } = null!; 
    }
}
