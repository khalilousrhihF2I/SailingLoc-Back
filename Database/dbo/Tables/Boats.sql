CREATE TABLE [dbo].[Boats] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL CHECK ([Type] IN ('sailboat', 'catamaran', 'motor', 'semirigid')),
    [Location] NVARCHAR(200) NOT NULL,
    [City] NVARCHAR(200) NOT NULL,
    [DestinationId] INT NULL,
    [Country] NVARCHAR(100) NOT NULL,
    [Price] DECIMAL(10,2) NOT NULL,
    [Capacity] INT NOT NULL,
    [Cabins] INT NOT NULL DEFAULT 0,
    [Length] DECIMAL(5,2) NOT NULL, -- En mètres
    [Year] INT NOT NULL,
    [Image] NVARCHAR(500) NULL,
    [Rating] DECIMAL(3,2) NOT NULL DEFAULT 0,
    [ReviewCount] INT NOT NULL DEFAULT 0,
    [Equipment] NVARCHAR(MAX) NULL, -- JSON array: ["GPS", "Pilote automatique", ...]
    [Description] NVARCHAR(MAX) NULL,
    
    -- Propriétaire
    [OwnerId]  UNIQUEIDENTIFIER   NOT NULL,
    
    -- Statut
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    
    -- Dates
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NULL,
    
    CONSTRAINT [FK_Boats_AspNetUsers_OwnerId] 
        FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_Boats_Destinations_DestinationId] 
        FOREIGN KEY ([DestinationId]) REFERENCES [dbo].[Destinations] ([Id]) ON DELETE SET NULL
);
GO

CREATE INDEX [IX_Boats_OwnerId] ON [dbo].[Boats] ([OwnerId]);
GO
CREATE INDEX [IX_Boats_Type] ON [dbo].[Boats] ([Type]);
GO
CREATE INDEX [IX_Boats_Location] ON [dbo].[Boats] ([Location]);
GO
CREATE INDEX [IX_Boats_DestinationId] ON [dbo].[Boats] ([DestinationId]);
GO
CREATE INDEX [IX_Boats_Price] ON [dbo].[Boats] ([Price]);
GO
CREATE INDEX [IX_Boats_IsActive] ON [dbo].[Boats] ([IsActive]);
GO