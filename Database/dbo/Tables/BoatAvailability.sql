
CREATE TABLE [dbo].[BoatAvailability] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    -- FK vers Boats
    [BoatId] INT NOT NULL,

    -- Dates en DATETIME2 pour correspondre à DateTime C#
    [StartDate] DATETIME2(7) NOT NULL,
    [EndDate] DATETIME2(7) NOT NULL,

    -- Disponibilité (true/false)
    [IsAvailable] BIT NOT NULL DEFAULT 1,

    -- Champs supplémentaires correspondant à ton entity
    [Reason] NVARCHAR(500) NULL,
    [ReferenceType] NVARCHAR(100) NULL,     -- ex: 'booking', 'blocked'
    [ReferenceId] NVARCHAR(100) NULL,       -- ex: bookingId (string)
    [Details] NVARCHAR(2000) NULL,          -- notes optionnelles

    -- Meta
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    -- FK Boats
    CONSTRAINT [FK_BoatAvailability_Boats_BoatId] 
        FOREIGN KEY ([BoatId]) REFERENCES [dbo].[Boats] ([Id]) 
        ON DELETE CASCADE
);
GO

-- Index pour les requêtes par bateau
CREATE INDEX [IX_BoatAvailability_BoatId] 
ON [dbo].[BoatAvailability] ([BoatId]);
GO

-- Index pour les plages de dates (optimise checkAvailability)
CREATE INDEX [IX_BoatAvailability_Dates] 
ON [dbo].[BoatAvailability] ([StartDate], [EndDate]);
GO

-- Index pour ReferenceType/ReferenceId (utile pour relations avec Booking)
CREATE INDEX [IX_BoatAvailability_Reference] 
ON [dbo].[BoatAvailability] ([ReferenceType], [ReferenceId]);
GO