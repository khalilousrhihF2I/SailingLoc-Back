CREATE TABLE [dbo].[RefreshTokens] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [Token]     NVARCHAR (512)   NOT NULL,
    [ExpiresAt] DATETIME2 (7)    NOT NULL,
    [Revoked]   BIT              CONSTRAINT [DF_RefreshTokens_Revoked] DEFAULT ((0)) NOT NULL,
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 (7)    CONSTRAINT [DF_RefreshTokens_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_RefreshTokens_Token]
    ON [dbo].[RefreshTokens]([Token] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId]
    ON [dbo].[RefreshTokens]([UserId] ASC);

