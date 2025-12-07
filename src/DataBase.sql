/* =========================================================
   StartupApi — Schéma SQL Server pour .NET 8 API (Identity + Custom)
   ========================================================= */

------------------------------------------------------------
-- 0) Créer la base (optionnel : adaptez le nom si besoin)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'StartupApi')
BEGIN
    CREATE DATABASE [StartupApi];
END
GO

USE [StartupApi];
GO

------------------------------------------------------------
-- 1) Tables ASP.NET Core Identity (GUID keys)
--    Noms et schéma standards (dbo)
------------------------------------------------------------

-- Roles
IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AspNetRoles
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
END
GO

-- Users (custom columns ajoutés)
IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AspNetUsers
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL CONSTRAINT DF_AspNetUsers_EmailConfirmed DEFAULT(0),
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL CONSTRAINT DF_AspNetUsers_PhoneNumberConfirmed DEFAULT(0),
    TwoFactorEnabled BIT NOT NULL CONSTRAINT DF_AspNetUsers_TwoFactorEnabled DEFAULT(0),
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL CONSTRAINT DF_AspNetUsers_LockoutEnabled DEFAULT(0),
    AccessFailedCount INT NOT NULL CONSTRAINT DF_AspNetUsers_AccessFailedCount DEFAULT(0),

    -- Champs métier supplémentaires (AppUser)
    FirstName NVARCHAR(100) NOT NULL CONSTRAINT DF_AspNetUsers_FirstName DEFAULT(N''),
    LastName  NVARCHAR(100) NOT NULL CONSTRAINT DF_AspNetUsers_LastName  DEFAULT(N''),
    BirthDate DATETIME2 NOT NULL CONSTRAINT DF_AspNetUsers_BirthDate DEFAULT (SYSUTCDATETIME()),
    -- Address (owned)
    Address_Street     NVARCHAR(256) NOT NULL CONSTRAINT DF_AspNetUsers_Address_Street DEFAULT(N''),
    Address_City       NVARCHAR(128) NOT NULL CONSTRAINT DF_AspNetUsers_Address_City   DEFAULT(N''),
    Address_State      NVARCHAR(128) NOT NULL CONSTRAINT DF_AspNetUsers_Address_State  DEFAULT(N''),
    Address_PostalCode NVARCHAR(32)  NOT NULL CONSTRAINT DF_AspNetUsers_Address_Zip    DEFAULT(N''),
    Address_Country    NVARCHAR(128) NOT NULL CONSTRAINT DF_AspNetUsers_Address_Ctry   DEFAULT(N''),

    -- 0=Active,1=Inactive,2=Pending,3=Banned
    Status INT NOT NULL CONSTRAINT DF_AspNetUsers_Status DEFAULT(0),
    AvatarUrl NVARCHAR(512) NULL,

    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AspNetUsers_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_AspNetUsers_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    LastLoginAt DATETIME2 NULL
);
CREATE UNIQUE INDEX EmailIndex ON dbo.AspNetUsers(NormalizedEmail);
CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;

-- Contrainte simple sur Status
ALTER TABLE dbo.AspNetUsers WITH CHECK ADD CONSTRAINT CK_AspNetUsers_Status CHECK (Status IN (0,1,2,3));
END
GO

-- RoleClaims
IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AspNetRoleClaims
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL
);
ALTER TABLE dbo.AspNetRoleClaims WITH CHECK ADD CONSTRAINT FK_AspNetRoleClaims_Roles_RoleId
    FOREIGN KEY(RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE;
END
GO

-- UserClaims
IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AspNetUserClaims
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetUserClaims PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL
);
ALTER TABLE dbo.AspNetUserClaims WITH CHECK ADD CONSTRAINT FK_AspNetUserClaims_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
END
GO

-- UserLogins (clé composite par défaut Identity: LoginProvider+ProviderKey)
IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AspNetUserLogins
(
    LoginProvider NVARCHAR(450) NOT NULL,
    ProviderKey NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(256) NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey)
);
CREATE INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins(UserId);
ALTER TABLE dbo.AspNetUserLogins WITH CHECK ADD CONSTRAINT FK_AspNetUserLogins_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
END
GO

-- UserRoles (clé composite)
IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AspNetUserRoles
(
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId)
);
CREATE INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles(RoleId);
ALTER TABLE dbo.AspNetUserRoles WITH CHECK ADD CONSTRAINT FK_AspNetUserRoles_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
ALTER TABLE dbo.AspNetUserRoles WITH CHECK ADD CONSTRAINT FK_AspNetUserRoles_Roles_RoleId
    FOREIGN KEY(RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE;
END
GO

-- UserTokens (clé composite)
IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AspNetUserTokens
(
    UserId UNIQUEIDENTIFIER NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name NVARCHAR(450) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name)
);
ALTER TABLE dbo.AspNetUserTokens WITH CHECK ADD CONSTRAINT FK_AspNetUserTokens_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
END
GO

------------------------------------------------------------
-- 2) Tables Custom (matching Infrastructure/Data & Entities)
------------------------------------------------------------

-- RefreshTokens
IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NULL
BEGIN
CREATE TABLE dbo.RefreshTokens
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
    Token NVARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    Revoked BIT NOT NULL CONSTRAINT DF_RefreshTokens_Revoked DEFAULT(0),
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT (SYSUTCDATETIME())
);
CREATE UNIQUE INDEX UX_RefreshTokens_Token ON dbo.RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
ALTER TABLE dbo.RefreshTokens WITH CHECK ADD CONSTRAINT FK_RefreshTokens_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
END
GO

-- PasswordResetTokens
IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
CREATE TABLE dbo.PasswordResetTokens
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
    Token NVARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    Used BIT NOT NULL CONSTRAINT DF_PasswordResetTokens_Used DEFAULT(0),
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAt DEFAULT (SYSUTCDATETIME())
);
CREATE UNIQUE INDEX UX_PasswordResetTokens_Token ON dbo.PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_UserId ON dbo.PasswordResetTokens(UserId);
ALTER TABLE dbo.PasswordResetTokens WITH CHECK ADD CONSTRAINT FK_PasswordResetTokens_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
END
GO

-- ExternalLogins (en plus de AspNetUserLogins si vous stockez des infos complémentaires)
IF OBJECT_ID(N'dbo.ExternalLogins', N'U') IS NULL
BEGIN
CREATE TABLE dbo.ExternalLogins
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ExternalLogins PRIMARY KEY,
    Provider NVARCHAR(128) NOT NULL,
    ProviderKey NVARCHAR(256) NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ExternalLogins_CreatedAt DEFAULT (SYSUTCDATETIME())
);
CREATE UNIQUE INDEX UX_ExternalLogins_Provider_ProviderKey ON dbo.ExternalLogins(Provider, ProviderKey);
CREATE INDEX IX_ExternalLogins_UserId ON dbo.ExternalLogins(UserId);
ALTER TABLE dbo.ExternalLogins WITH CHECK ADD CONSTRAINT FK_ExternalLogins_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
END
GO

-- AuditLogs
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AuditLogs
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NULL,
    Action NVARCHAR(256) NOT NULL,
    Ip NVARCHAR(64) NOT NULL,
    Details NVARCHAR(MAX) NULL,
    Timestamp DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_Timestamp DEFAULT (SYSUTCDATETIME())
);
CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs(UserId);
ALTER TABLE dbo.AuditLogs WITH CHECK ADD CONSTRAINT FK_AuditLogs_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE SET NULL;
END
GO

-- Profiles (ressource future)
IF OBJECT_ID(N'dbo.Profiles', N'U') IS NULL
BEGIN
CREATE TABLE dbo.Profiles
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Profiles PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Bio NVARCHAR(1024) NULL
);
CREATE UNIQUE INDEX UX_Profiles_UserId ON dbo.Profiles(UserId);
ALTER TABLE dbo.Profiles WITH CHECK ADD CONSTRAINT FK_Profiles_Users_UserId
    FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE;
END
GO

------------------------------------------------------------
-- 3) Rôles de base (Admin, User) — l’API ajoute l’admin au démarrage
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = N'ADMIN')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), N'Admin', N'ADMIN', NEWID());
END

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = N'USER')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), N'User', N'USER', NEWID());
END
GO

/* =========================================================
   FIN
   ========================================================= */
