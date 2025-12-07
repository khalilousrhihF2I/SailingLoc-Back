
-- -----------------------------------------------------------------------------
-- Table Reviews
-- Stocke les avis sur les bateaux
-- -----------------------------------------------------------------------------
CREATE TABLE [dbo].[Reviews] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BoatId] INT NOT NULL,
    [BookingId] NVARCHAR(50) NULL,
    [UserId]  UNIQUEIDENTIFIER NOT NULL,
    [UserName] NVARCHAR(256) NOT NULL,
    [UserAvatar] NVARCHAR(500) NULL,
    
    -- Évaluation
    [Rating] INT NOT NULL CHECK ([Rating] >= 1 AND [Rating] <= 5),
    [Comment] NVARCHAR(MAX) NULL,
    
    -- Dates
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NULL,
    
    CONSTRAINT [FK_Reviews_Boats_BoatId] 
        FOREIGN KEY ([BoatId]) REFERENCES [dbo].[Boats] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reviews_Bookings_BookingId] 
        FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings] ([Id]),
    CONSTRAINT [FK_Reviews_AspNetUsers_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);
GO

CREATE INDEX [IX_Reviews_BoatId] ON [dbo].[Reviews] ([BoatId]);
GO
CREATE INDEX [IX_Reviews_UserId] ON [dbo].[Reviews] ([UserId]);
GO
CREATE INDEX [IX_Reviews_BookingId] ON [dbo].[Reviews] ([BookingId]);
GO
CREATE INDEX [IX_Reviews_CreatedAt] ON [dbo].[Reviews] ([CreatedAt]);
GO