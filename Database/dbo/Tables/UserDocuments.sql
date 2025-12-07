
-- -----------------------------------------------------------------------------
-- Table UserDocuments
-- Stocke les documents des utilisateurs (permis, pièces d'identité, etc.)
-- -----------------------------------------------------------------------------
CREATE TABLE [dbo].[UserDocuments] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId]  UNIQUEIDENTIFIER NOT NULL,
    [DocumentType] NVARCHAR(100) NOT NULL, -- Ex: "ID Card", "Boat License", "Insurance"
    [DocumentUrl] NVARCHAR(500) NOT NULL, 
    [BoatId] INT NULL,
    [FileName] NVARCHAR(256) NOT NULL,
    [FileSize] BIGINT NOT NULL, -- En bytes
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [VerifiedAt] DATETIME2(7) NULL,
    [VerifiedBy] UNIQUEIDENTIFIER NULL, -- Admin ID
    [UploadedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_UserDocuments_AspNetUsers_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserDocuments_AspNetUsers_VerifiedBy] 
        FOREIGN KEY ([VerifiedBy]) REFERENCES [dbo].[AspNetUsers] ([Id])
);
GO

CREATE INDEX [IX_UserDocuments_UserId] ON [dbo].[UserDocuments] ([UserId]);
GO
CREATE INDEX [IX_UserDocuments_DocumentType] ON [dbo].[UserDocuments] ([DocumentType]);
GO