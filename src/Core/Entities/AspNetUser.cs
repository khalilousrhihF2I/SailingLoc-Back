using System;
using System.Collections.Generic;

namespace Core.Entities
{
    /// <summary>
    /// Représente un utilisateur de la plateforme (locataire, propriétaire, admin).
    /// Basé sur la table AspNetUsers.
    /// </summary>
    public class AppUserssssss
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime BirthDate { get; set; }

        public string Address_Street { get; set; } = "";
        public string Address_City { get; set; } = "";
        public string Address_State { get; set; } = "";
        public string Address_PostalCode { get; set; } = "";
        public string Address_Country { get; set; } = "";

        public int Status { get; set; }
        public string UserType { get; set; } = "";
        public bool Verified { get; set; }
        public DateTime MemberSince { get; set; }
        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public ICollection<Boat> Boats { get; set; } = new List<Boat>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<UserDocument> UserDocuments { get; set; } = new List<UserDocument>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
        public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = new List<PasswordResetCode>();
        public Profile? Profile { get; set; }
    }
}
