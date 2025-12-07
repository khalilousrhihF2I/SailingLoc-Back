using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Emit;

namespace Infrastructure.Data
{
    /// <summary>
    /// Contexte EF Core de l'application.
    /// Hérite de IdentityDbContext pour gérer AppUser / AppRole + tout le métier.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // --- Identity extensions / sécurité ---
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
        public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Profile> Profiles => Set<Profile>();

        // --- Domaine Sailing / Loc ---
        public DbSet<Boat> Boats => Set<Boat>();
        public DbSet<BoatImage> BoatImages => Set<BoatImage>();
        public DbSet<BoatAvailability> BoatAvailabilities => Set<BoatAvailability>();
        public DbSet<Destination> Destinations => Set<Destination>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<UserDocument> UserDocuments => Set<UserDocument>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // -------------------------
            // AppUser & owned Address
            // -------------------------
            b.Entity<AppUser>(e =>
            {
                // Adresse en owned type => colonnes Address_*
                e.OwnsOne(u => u.Address);

                // Exemple : tu peux ajouter des contraintes ici si tu veux
                // e.Property(u => u.FirstName).HasMaxLength(100);
            });

            // -------------------------
            // RefreshToken
            // -------------------------
            b.Entity<RefreshToken>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                e.HasIndex(x => x.Token).IsUnique();

                e.HasOne(x => x.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // PasswordResetToken
            // -------------------------
            b.Entity<PasswordResetToken>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                e.HasIndex(x => x.Token).IsUnique();

                e.HasOne(x => x.User)
                    .WithMany(u => u.PasswordResetTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // ExternalLogin
            // -------------------------
            b.Entity<ExternalLogin>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Provider)
                    .IsRequired()
                    .HasMaxLength(128);

                e.Property(x => x.ProviderKey)
                    .IsRequired()
                    .HasMaxLength(256);

                e.HasIndex(x => new { x.Provider, x.ProviderKey })
                    .IsUnique();

                e.HasOne(x => x.User)
                    .WithMany(u => u.ExternalLogins)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // AuditLog
            // -------------------------
            b.Entity<AuditLog>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Action)
                    .IsRequired()
                    .HasMaxLength(256);

                e.Property(x => x.Ip)
                    .IsRequired()
                    .HasMaxLength(64);

                // FK optionnelle vers AppUser, ON DELETE SET NULL
                e.HasOne<AppUser>()
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // -------------------------
            // Profile (1-1 avec AppUser)
            // -------------------------
            b.Entity<Profile>(e =>
            {
                e.HasKey(x => x.Id);

            });

            // -------------------------
            // PasswordResetCode (OTP)
            // -------------------------
            b.Entity<PasswordResetCode>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.CodeHash)
                    .IsRequired()
                    .HasMaxLength(512);

                e.Property(x => x.Purpose)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValue("password-reset");

                e.Property(x => x.Used)
                    .HasDefaultValue(false);

                e.Property(x => x.Attempts)
                    .HasDefaultValue(0);

                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                e.HasOne(x => x.User)
                    .WithMany(u => u.PasswordResetCodes)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.UserId, x.Purpose, x.ExpiresAt })
                    .HasDatabaseName("IX_PasswordResetCodes_User_Purpose_Expires");

                e.HasIndex(x => new { x.UserId, x.Used, x.ExpiresAt })
                    .HasDatabaseName("IX_PasswordResetCodes_Active");
            });

            // -------------------------
            // Boat
            // -------------------------
            b.Entity<Boat>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Type).IsRequired().HasMaxLength(50);
                e.Property(x => x.Location).IsRequired().HasMaxLength(200);
                e.Property(x => x.City).IsRequired().HasMaxLength(200);
                e.Property(x => x.Country).IsRequired().HasMaxLength(100);

                e.HasOne(x => x.Owner)
                    .WithMany(u => u.BoatsOwned)
                    .HasForeignKey(x => x.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Destination)
                    .WithMany(d => d.Boats)
                    .HasForeignKey(x => x.DestinationId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Global query filter to hide soft-deleted boats by default
                e.HasQueryFilter(x => !x.IsDeleted);
            });

            b.Entity<Boat>()
    .ToTable(tb => tb.HasTrigger("tr_Boats_AfterInsertUpdate"));


            // -------------------------
            // BoatImage
            // -------------------------
            b.Entity<BoatImage>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                e.HasOne(x => x.Boat)
                    .WithMany(b => b.Images)
                    .HasForeignKey(x => x.BoatId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // BoatAvailability
            // -------------------------
            b.Entity<BoatAvailability>(e =>
            {
                e.ToTable("BoatAvailability");
                e.HasKey(x => x.Id);

                e.HasOne(x => x.Boat)
                    .WithMany(b => b.Availabilities)
                    .HasForeignKey(x => x.BoatId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // Destination
            // -------------------------
            b.Entity<Destination>(e =>
            {
                e.HasKey(x => x.Id);
            });

            // -------------------------
            // Booking
            // -------------------------
            b.Entity<Booking>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasOne(x => x.Boat)
                    .WithMany(b => b.Bookings)
                    .HasForeignKey(x => x.BoatId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Renter)
                   .WithMany(u => u.Bookings)
                   .HasForeignKey(x => x.RenterId)
                   .OnDelete(DeleteBehavior.Restrict);


            });

            // -------------------------
            // Message
            // -------------------------
            b.Entity<Message>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasOne(x => x.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(x => x.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Receiver)
                    .WithMany(u => u.ReceivedMessages)
                    .HasForeignKey(x => x.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Boat)
                    .WithMany(b => b.Messages)
                    .HasForeignKey(x => x.BoatId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(x => x.Booking)
                    .WithMany(b => b.Messages)
                    .HasForeignKey(x => x.BookingId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // -------------------------
            // Review
            // -------------------------
            b.Entity<Review>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasOne(x => x.Boat)
                    .WithMany(b => b.Reviews)
                    .HasForeignKey(x => x.BoatId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
     .WithMany(u => u.Reviews)
     .HasForeignKey(x => x.UserId)
     .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Booking)
                    .WithMany(b => b.Reviews)
                    .HasForeignKey(x => x.BookingId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            b.Entity<Review>()
        .ToTable(tb => tb.HasTrigger("tr_Reviews_AfterInsert"));
            // -------------------------
            // UserDocument
            // -------------------------
            b.Entity<UserDocument>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.DocumentType)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(x => x.DocumentUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                e.Property(x => x.FileName)
                    .IsRequired()
                    .HasMaxLength(256);

                // -----------------------------
                // Relation : User → UserDocuments
                // -----------------------------
                e.HasOne(x => x.User)
                    .WithMany(u => u.UserDocuments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // -----------------------------
                // Relation : Admin → DocumentsVerified
                // -----------------------------
                e.HasOne(x => x.VerifiedByUser)
                    .WithMany(u => u.DocumentsVerified)
                    .HasForeignKey(x => x.VerifiedBy)
                    .OnDelete(DeleteBehavior.NoAction);
            });

        }
    }
}
