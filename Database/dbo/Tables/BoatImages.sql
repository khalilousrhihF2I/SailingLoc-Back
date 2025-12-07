
-- -----------------------------------------------------------------------------
-- Table BoatImages
-- Stocke les images supplémentaires des bateaux
-- -----------------------------------------------------------------------------
CREATE TABLE [dbo].[BoatImages] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BoatId] INT NOT NULL,
    [ImageUrl] NVARCHAR(500) NOT NULL,
    [Caption] NVARCHAR(500) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_BoatImages_Boats_BoatId] 
        FOREIGN KEY ([BoatId]) REFERENCES [dbo].[Boats] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_BoatImages_BoatId] ON [dbo].[BoatImages] ([BoatId]);
GO