-- -----------------------------------------------------------------------------
-- Table BoatPricePeriods
-- Tarification saisonnière : prix différent par période (haute/basse saison)
-- -----------------------------------------------------------------------------
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
GO

CREATE INDEX [IX_BoatPricePeriods_BoatId] ON [dbo].[BoatPricePeriods] ([BoatId]);
GO
