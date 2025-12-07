CREATE TABLE [dbo].[Profiles] (
    [Id]     UNIQUEIDENTIFIER NOT NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Bio]    NVARCHAR (1024)  NULL,
    CONSTRAINT [PK_Profiles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Profiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Profiles_UserId]
    ON [dbo].[Profiles]([UserId] ASC);

