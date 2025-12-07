-- -----------------------------------------------------------------------------
-- Table Bookings
-- Stocke les réservations de bateaux
-- -----------------------------------------------------------------------------
CREATE TABLE [dbo].[Bookings] (
    [Id] NVARCHAR(50) NOT NULL PRIMARY KEY, -- Format: BK{timestamp}
    [BoatId] INT NOT NULL,
    [RenterId] UNIQUEIDENTIFIER NOT NULL,
    
    -- Dates
    [StartDate] DATE NOT NULL,
    [EndDate] DATE NOT NULL,
    [Days] AS DATEDIFF(DAY, [StartDate], [EndDate]) PERSISTED,
    
    -- Prix
    [DailyPrice] DECIMAL(10,2) NOT NULL,
    [Subtotal] DECIMAL(10,2) NOT NULL,
    [ServiceFee] DECIMAL(10,2) NOT NULL,
    [TotalPrice] DECIMAL(10,2) NOT NULL,
    
    -- Statut
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending' 
        CHECK ([Status] IN ('pending', 'confirmed', 'completed', 'cancelled')),
    
    -- Informations locataire
    [RenterName] NVARCHAR(256) NOT NULL,
    [RenterEmail] NVARCHAR(256) NOT NULL,
    [RenterPhone] NVARCHAR(50) NULL,
    
    -- Paiement
    [PaymentIntentId] NVARCHAR(200) NULL, -- Stripe Payment Intent ID
    [PaymentStatus] NVARCHAR(50) NOT NULL DEFAULT 'pending'
        CHECK ([PaymentStatus] IN ('pending', 'succeeded', 'failed', 'refunded')),
    [PaidAt] DATETIME2(7) NULL,
    
    -- Dates système
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NULL,
    [CancelledAt] DATETIME2(7) NULL,
    
    CONSTRAINT [FK_Bookings_Boats_BoatId] 
        FOREIGN KEY ([BoatId]) REFERENCES [dbo].[Boats] ([Id]),
    CONSTRAINT [FK_Bookings_AspNetUsers_RenterId] 
        FOREIGN KEY ([RenterId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);
GO

CREATE INDEX [IX_Bookings_BoatId] ON [dbo].[Bookings] ([BoatId]);
GO
CREATE INDEX [IX_Bookings_RenterId] ON [dbo].[Bookings] ([RenterId]);
GO
CREATE INDEX [IX_Bookings_Status] ON [dbo].[Bookings] ([Status]);
GO
CREATE INDEX [IX_Bookings_StartDate] ON [dbo].[Bookings] ([StartDate]);
GO
CREATE INDEX [IX_Bookings_EndDate] ON [dbo].[Bookings] ([EndDate]);
GO
CREATE INDEX [IX_Bookings_CreatedAt] ON [dbo].[Bookings] ([CreatedAt]);
GO