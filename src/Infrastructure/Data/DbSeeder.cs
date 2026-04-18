using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var rm = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                 ?? config["Seed:AdminPassword"]
                 ?? throw new Exception("ADMIN_PASSWORD missing!");

        var renterPass = Environment.GetEnvironmentVariable("RENTER_PASSWORD")
                         ?? config["Seed:RenterPassword"]
                         ?? throw new Exception("RENTER_PASSWORD missing!");

        var ownerPass = Environment.GetEnvironmentVariable("OWNER_PASSWORD")
                         ?? config["Seed:OwnerPassword"]
                         ?? throw new Exception("OWNER_PASSWORD missing!");

        // Apply incremental SQL migrations (idempotent — safe to run multiple times)
        await ApplyIncrementalMigrationsAsync(db, logger);

        // --- Roles ---
        var roles = new[] { "Admin", "Renter", "Owner" };
        foreach (var role in roles)
        {
            if (!await rm.RoleExistsAsync(role))
                await rm.CreateAsync(new AppRole { Name = role });
        }
        // --- SailingLoc ---
        if (await um.FindByEmailAsync("voisinad7373@gmail.com") is null)
        {
            var admin = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "voisinad7373@gmail.com",
                UserName = "voisinad7373",
                FirstName = "Ousrhih",
                LastName = "Khalil",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                UserType = "admin",
                EmailConfirmed = true
            };
            var result1 = await um.CreateAsync(admin, adminPass);
            if (result1.Succeeded)
            {
                var created = await um.FindByEmailAsync(admin.Email!);
                await um.AddToRoleAsync(created!, "Admin");
            }
        }

        // --- Admin ---
        if (await um.FindByEmailAsync("admin@local.test") is null)
        {
            var admin = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "admin@local.test",
                UserName = "admin@local.test",
                FirstName = "Admin",
                LastName = "User",
                BirthDate = DateTime.UtcNow.AddYears(-20),
                UserType = "admin",
                EmailConfirmed = true
            };
            var result2 = await um.CreateAsync(admin, adminPass);
            if (result2.Succeeded)
            {
                var created = await um.FindByEmailAsync(admin.Email!);
                await um.AddToRoleAsync(created!, "Admin");
            }
        }

        // --- Client ---
        if (await um.FindByEmailAsync("Renter@local.test") is null)
        {
            var client = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "Renter@local.test",
                UserName = "Renter@local.test",
                FirstName = "Renter",
                LastName = "User",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                UserType = "renter",
                EmailConfirmed = true
            };
            var result3 = await um.CreateAsync(client, renterPass);
            if (result3.Succeeded)
            {
                var created = await um.FindByEmailAsync(client.Email!);
                await um.AddToRoleAsync(created!, "Renter");
            }
        }

        // --- Partner ---
        if (await um.FindByEmailAsync("Owner@local.test") is null)
        {
            var partner = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "Owner@local.test",
                UserName = "Owner@local.test",
                FirstName = "Owner",
                LastName = "User",
                BirthDate = DateTime.UtcNow.AddYears(-35),
                UserType = "owner",
                EmailConfirmed = true
            };
            var result4 = await um.CreateAsync(partner, ownerPass);
            if (result4.Succeeded)
            {
                var created = await um.FindByEmailAsync(partner.Email!);
                await um.AddToRoleAsync(created!, "Owner");
            }
        }
    }

    /// <summary>
    /// Runs embedded SQL migration scripts against the database.
    /// Each statement is idempotent (IF NOT EXISTS guards) so safe to run repeatedly.
    /// </summary>
    private static async Task ApplyIncrementalMigrationsAsync(ApplicationDbContext db, ILogger logger)
    {
        // Ensure database exists (no-op if it does)
        await db.Database.EnsureCreatedAsync();

        var migrationSql = @"
-- ─── 1. Boats: Add skipper option columns ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Boats') AND name = 'HasSkipper')
    ALTER TABLE [dbo].[Boats] ADD [HasSkipper] BIT NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Boats') AND name = 'SkipperPrice')
    ALTER TABLE [dbo].[Boats] ADD [SkipperPrice] DECIMAL(10,2) NOT NULL DEFAULT 0;

-- ─── 2. Bookings: Add skipper fee columns ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'WithSkipper')
    ALTER TABLE [dbo].[Bookings] ADD [WithSkipper] BIT NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'SkipperFee')
    ALTER TABLE [dbo].[Bookings] ADD [SkipperFee] DECIMAL(10,2) NOT NULL DEFAULT 0;

-- ─── 3. Reviews: Add moderation workflow columns ────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModerationStatus')
    ALTER TABLE [dbo].[Reviews] ADD [ModerationStatus] NVARCHAR(50) NOT NULL DEFAULT 'pending';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModerationNote')
    ALTER TABLE [dbo].[Reviews] ADD [ModerationNote] NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModeratedBy')
    ALTER TABLE [dbo].[Reviews] ADD [ModeratedBy] UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModeratedAt')
    ALTER TABLE [dbo].[Reviews] ADD [ModeratedAt] DATETIME2(7) NULL;

-- ─── 4. Create BoatPricePeriods table (seasonal pricing) ────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BoatPricePeriods')
BEGIN
    CREATE TABLE [dbo].[BoatPricePeriods] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BoatId] INT NOT NULL,
        [Label] NVARCHAR(100) NOT NULL,
        [StartDate] DATETIME2(7) NOT NULL,
        [EndDate] DATETIME2(7) NOT NULL,
        [PricePerDay] DECIMAL(18,2) NOT NULL,
        CONSTRAINT [FK_BoatPricePeriods_Boats_BoatId]
            FOREIGN KEY ([BoatId]) REFERENCES [dbo].[Boats] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_BoatPricePeriods_BoatId] ON [dbo].[BoatPricePeriods] ([BoatId]);
END

-- ─── 5. Create Disputes table (litigation management) ──────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Disputes')
BEGIN
    CREATE TABLE [dbo].[Disputes] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BookingId] NVARCHAR(50) NOT NULL,
        [ReporterId] UNIQUEIDENTIFIER NOT NULL,
        [RespondentId] UNIQUEIDENTIFIER NULL,
        [Subject] NVARCHAR(300) NOT NULL,
        [Description] NVARCHAR(4000) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'open',
        [Resolution] NVARCHAR(MAX) NULL,
        [AdminNote] NVARCHAR(MAX) NULL,
        [ResolvedBy] UNIQUEIDENTIFIER NULL,
        [ResolvedAt] DATETIME2(7) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        CONSTRAINT [FK_Disputes_Bookings_BookingId]
            FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings] ([Id]),
        CONSTRAINT [FK_Disputes_AspNetUsers_ReporterId]
            FOREIGN KEY ([ReporterId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );
    CREATE INDEX [IX_Disputes_BookingId] ON [dbo].[Disputes] ([BookingId]);
    CREATE INDEX [IX_Disputes_ReporterId] ON [dbo].[Disputes] ([ReporterId]);
    CREATE INDEX [IX_Disputes_Status] ON [dbo].[Disputes] ([Status]);
END
";

        try
        {
            await db.Database.ExecuteSqlRawAsync(migrationSql);
            logger.LogInformation("Incremental SQL migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Incremental SQL migration warning (may be partially applied): {Message}", ex.Message);
        }
    }
}
