-- =============================================================================
-- Migration 002: Add Skipper, Seasonal Pricing, Review Moderation, Disputes
-- Date: 2026-03-30
-- =============================================================================

-- ─── 1. Boats: Add skipper option columns ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Boats') AND name = 'HasSkipper')
BEGIN
    ALTER TABLE [dbo].[Boats] ADD [HasSkipper] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Boats') AND name = 'SkipperPrice')
BEGIN
    ALTER TABLE [dbo].[Boats] ADD [SkipperPrice] DECIMAL(10,2) NOT NULL DEFAULT 0;
END
GO

-- ─── 2. Bookings: Add skipper fee columns ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'WithSkipper')
BEGIN
    ALTER TABLE [dbo].[Bookings] ADD [WithSkipper] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'SkipperFee')
BEGIN
    ALTER TABLE [dbo].[Bookings] ADD [SkipperFee] DECIMAL(10,2) NOT NULL DEFAULT 0;
END
GO

-- ─── 3. Reviews: Add moderation workflow columns ────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModerationStatus')
BEGIN
    ALTER TABLE [dbo].[Reviews] ADD [ModerationStatus] NVARCHAR(50) NOT NULL DEFAULT 'pending';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModerationNote')
BEGIN
    ALTER TABLE [dbo].[Reviews] ADD [ModerationNote] NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModeratedBy')
BEGIN
    ALTER TABLE [dbo].[Reviews] ADD [ModeratedBy] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Reviews') AND name = 'ModeratedAt')
BEGIN
    ALTER TABLE [dbo].[Reviews] ADD [ModeratedAt] DATETIME2(7) NULL;
END
GO

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
GO

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
GO
