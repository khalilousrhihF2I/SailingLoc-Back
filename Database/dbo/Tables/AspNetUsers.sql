CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   UNIQUEIDENTIFIER   NOT NULL,
    [UserName]             NVARCHAR (256)     NULL,
    [NormalizedUserName]   NVARCHAR (256)     NULL,
    [Email]                NVARCHAR (256)     NULL,
    [NormalizedEmail]      NVARCHAR (256)     NULL,
    [EmailConfirmed]       BIT                CONSTRAINT [DF_AspNetUsers_EmailConfirmed] DEFAULT ((0)) NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)     NULL,
    [SecurityStamp]        NVARCHAR (MAX)     NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
    [PhoneNumber]          NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed] BIT                CONSTRAINT [DF_AspNetUsers_PhoneNumberConfirmed] DEFAULT ((0)) NOT NULL,
    [TwoFactorEnabled]     BIT                CONSTRAINT [DF_AspNetUsers_TwoFactorEnabled] DEFAULT ((0)) NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]       BIT                CONSTRAINT [DF_AspNetUsers_LockoutEnabled] DEFAULT ((0)) NOT NULL,
    [AccessFailedCount]    INT                CONSTRAINT [DF_AspNetUsers_AccessFailedCount] DEFAULT ((0)) NOT NULL,
    [FirstName]            NVARCHAR (100)     CONSTRAINT [DF_AspNetUsers_FirstName] DEFAULT (N'') NOT NULL,
    [LastName]             NVARCHAR (100)     CONSTRAINT [DF_AspNetUsers_LastName] DEFAULT (N'') NOT NULL,
    [BirthDate]            DATETIME2 (7)      CONSTRAINT [DF_AspNetUsers_BirthDate] DEFAULT (sysutcdatetime()) NOT NULL,
    [Address_Street]       NVARCHAR (256)     CONSTRAINT [DF_AspNetUsers_Address_Street] DEFAULT (N'') NOT NULL,
    [Address_City]         NVARCHAR (128)     CONSTRAINT [DF_AspNetUsers_Address_City] DEFAULT (N'') NOT NULL,
    [Address_State]        NVARCHAR (128)     CONSTRAINT [DF_AspNetUsers_Address_State] DEFAULT (N'') NOT NULL,
    [Address_PostalCode]   NVARCHAR (32)      CONSTRAINT [DF_AspNetUsers_Address_Zip] DEFAULT (N'') NOT NULL,
    [Address_Country]      NVARCHAR (128)     CONSTRAINT [DF_AspNetUsers_Address_Ctry] DEFAULT (N'') NOT NULL,
    [Status]               INT                CONSTRAINT [DF_AspNetUsers_Status] DEFAULT ((0)) NOT NULL,
    [UserType] NVARCHAR(50) NOT NULL CHECK ([UserType] IN ('renter', 'owner', 'admin')),
    [Verified] BIT NOT NULL DEFAULT 0,
    [MemberSince] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [AvatarUrl]            NVARCHAR (512)     NULL,
    [CreatedAt]            DATETIME2 (7)      CONSTRAINT [DF_AspNetUsers_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]            DATETIME2 (7)      CONSTRAINT [DF_AspNetUsers_UpdatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [LastLoginAt]          DATETIME2 (7)      NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_AspNetUsers_Status] CHECK ([Status]=(3) OR [Status]=(2) OR [Status]=(1) OR [Status]=(0))
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);

