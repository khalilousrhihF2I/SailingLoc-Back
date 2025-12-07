CREATE TABLE [dbo].[PasswordResetTokens] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [Token]     NVARCHAR (512)   NOT NULL,
    [ExpiresAt] DATETIME2 (7)    NOT NULL,
    [Used]      BIT              CONSTRAINT [DF_PasswordResetTokens_Used] DEFAULT ((0)) NOT NULL,
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 (7)    CONSTRAINT [DF_PasswordResetTokens_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_PasswordResetTokens_Token]
    ON [dbo].[PasswordResetTokens]([Token] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PasswordResetTokens_UserId]
    ON [dbo].[PasswordResetTokens]([UserId] ASC);

