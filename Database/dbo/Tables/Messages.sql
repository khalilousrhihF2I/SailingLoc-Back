
-- -----------------------------------------------------------------------------
-- Table Messages
-- Stocke les messages entre propriétaires et locataires
-- -----------------------------------------------------------------------------
CREATE TABLE [dbo].[Messages] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SenderId] UNIQUEIDENTIFIER NOT NULL,
    [ReceiverId]  UNIQUEIDENTIFIER NOT NULL,
    [BookingId] NVARCHAR(50) NULL,
    [BoatId] INT NULL,
    [Subject] NVARCHAR(500) NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [IsRead] BIT NOT NULL DEFAULT 0,
    [ReadAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_Messages_AspNetUsers_SenderId] 
        FOREIGN KEY ([SenderId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_Messages_AspNetUsers_ReceiverId] 
        FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_Messages_Bookings_BookingId] 
        FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings] ([Id]),
    CONSTRAINT [FK_Messages_Boats_BoatId] 
        FOREIGN KEY ([BoatId]) REFERENCES [dbo].[Boats] ([Id])
);
GO 
CREATE INDEX [IX_Messages_SenderId] ON [dbo].[Messages] ([SenderId]);
GO
CREATE INDEX [IX_Messages_ReceiverId] ON [dbo].[Messages] ([ReceiverId]);
GO
CREATE INDEX [IX_Messages_BookingId] ON [dbo].[Messages] ([BookingId]);
GO
CREATE INDEX [IX_Messages_CreatedAt] ON [dbo].[Messages] ([CreatedAt]);
GO