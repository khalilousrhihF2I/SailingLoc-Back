CREATE TABLE [dbo].[AuditLogs] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [UserId]    UNIQUEIDENTIFIER NULL,
    [Action]    NVARCHAR (256)   NOT NULL,
    [Ip]        NVARCHAR (64)    NOT NULL,
    [Details]   NVARCHAR (MAX)   NULL,
    [Timestamp] DATETIME2 (7)    CONSTRAINT [DF_AuditLogs_Timestamp] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId]
    ON [dbo].[AuditLogs]([UserId] ASC);

