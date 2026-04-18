-- -----------------------------------------------------------------------------
-- Table Disputes
-- Gestion des litiges entre locataires et propriétaires
-- -----------------------------------------------------------------------------
CREATE TABLE [dbo].[Disputes] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BookingId] NVARCHAR(50) NOT NULL,
    [ReporterId] UNIQUEIDENTIFIER NOT NULL,
    [RespondentId] UNIQUEIDENTIFIER NULL,

    -- Contenu
    [Subject] NVARCHAR(300) NOT NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'open'
        CHECK ([Status] IN ('open', 'under_review', 'resolved', 'closed')),

    -- Résolution
    [Resolution] NVARCHAR(MAX) NULL,
    [AdminNote] NVARCHAR(MAX) NULL,
    [ResolvedBy] UNIQUEIDENTIFIER NULL,
    [ResolvedAt] DATETIME2(7) NULL,

    -- Dates système
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NULL,

    CONSTRAINT [FK_Disputes_Bookings_BookingId]
        FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings] ([Id]),
    CONSTRAINT [FK_Disputes_AspNetUsers_ReporterId]
        FOREIGN KEY ([ReporterId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);
GO

CREATE INDEX [IX_Disputes_BookingId] ON [dbo].[Disputes] ([BookingId]);
GO
CREATE INDEX [IX_Disputes_ReporterId] ON [dbo].[Disputes] ([ReporterId]);
GO
CREATE INDEX [IX_Disputes_Status] ON [dbo].[Disputes] ([Status]);
GO
