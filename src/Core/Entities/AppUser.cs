using Core.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Core.Entities
{
    /// <summary>
    /// Représente un utilisateur (locataire, propriétaire ou admin).
    /// FUSION COMPLÈTE : IdentityUser<Guid> + AspNetUser (table SQL) + Navigations métier.
    /// </summary>
    public class AppUser : IdentityUser<Guid>
    {
        // ---------------------------
        // Données personnelles
        // ---------------------------

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime BirthDate { get; set; }

        // Owned type => Address_Street, Address_City, etc.
        public Address Address { get; set; } = new();

        // ---------------------------
        // Champs métier provenant SQL
        // ---------------------------

        /// <summary>Status général de l'utilisateur (enum remplaçant Status int).</summary>
        public UserStatus Status { get; set; } = UserStatus.Active;

        /// <summary>renter / owner / admin</summary>
        public string UserType { get; set; } = "";

        /// <summary>Compte validé manuellement (KYC, documents, etc.).</summary>
        public bool Verified { get; set; }

        /// <summary>Date d’inscription (MemberSince SQL).</summary>
        public DateTime MemberSince { get; set; } = DateTime.UtcNow;

        /// <summary>Avatar du profil utilisateur.</summary>
        public string? AvatarUrl { get; set; }

        // ---------------------------
        // Dates système
        // ---------------------------

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // ---------------------------
        // Navigations Identity
        // ---------------------------

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public ICollection<ExternalLogin> ExternalLogins { get; set; } = [];
        public ICollection<AuditLog> AuditLogs { get; set; } = [];
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
        public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = [];

        // ---------------------------
        // Navigations METIER SailingLoc
        // ---------------------------

        /// <summary>Bateaux dont l’utilisateur est propriétaire.</summary>
        public ICollection<Boat> BoatsOwned { get; set; } = [];

        /// <summary>Réservations effectuées par l’utilisateur en tant que locataire.</summary>
        public ICollection<Booking> Bookings { get; set; } = [];

        /// <summary>Messages envoyés.</summary>
        public ICollection<Message> SentMessages { get; set; } = [];

        /// <summary>Messages reçus.</summary>
        public ICollection<Message> ReceivedMessages { get; set; } = [];

        /// <summary>Avis laissés par l’utilisateur sur des bateaux.</summary>
        public ICollection<Review> Reviews { get; set; } = [];

        /// <summary>Documents uploadés (CNI, permis bateau, assurance...).</summary>
        public ICollection<UserDocument> UserDocuments { get; set; } = [];

        /// <summary>Documents que cet utilisateur (admin) a vérifiés.</summary>
        public ICollection<UserDocument> DocumentsVerified { get; set; } = [];
    }
}
