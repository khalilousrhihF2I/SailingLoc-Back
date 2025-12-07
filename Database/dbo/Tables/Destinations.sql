CREATE TABLE [dbo].[Destinations] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Region] NVARCHAR(200) NOT NULL,
    [Country] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Image] NVARCHAR(500) NULL,
    [AveragePrice] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [PopularMonths] NVARCHAR(500) NULL, -- JSON array: ["Juin", "Juillet", "Août"]
    [Highlights] NVARCHAR(MAX) NULL,    -- JSON array: ["Calanques", "Îles", ...]
    [BoatCount] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NULL
);
GO
CREATE INDEX [IX_Destinations_Country] ON [dbo].[Destinations] ([Country]);
GO
CREATE INDEX [IX_Destinations_Region] ON [dbo].[Destinations] ([Region]);
GO