CREATE TABLE [dbo].[ExternalLogins] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [Provider]    NVARCHAR (128)   NOT NULL,
    [ProviderKey] NVARCHAR (256)   NOT NULL,
    [UserId]      UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]   DATETIME2 (7)    CONSTRAINT [DF_ExternalLogins_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_ExternalLogins] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ExternalLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_ExternalLogins_Provider_ProviderKey]
    ON [dbo].[ExternalLogins]([Provider] ASC, [ProviderKey] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ExternalLogins_UserId]
    ON [dbo].[ExternalLogins]([UserId] ASC);

