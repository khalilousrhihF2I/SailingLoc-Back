CREATE TABLE dbo.PasswordResetCodes
(
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CodeHash NVARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2(7) NOT NULL,
    Used BIT NOT NULL DEFAULT 0,
    Attempts INT NOT NULL DEFAULT 0,
    Purpose NVARCHAR(100) NOT NULL DEFAULT N'password-reset',
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_PasswordResetCodes_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);

GO;
CREATE INDEX IX_PasswordResetCodes_User_Purpose_Expires
    ON dbo.PasswordResetCodes (UserId, Purpose, ExpiresAt DESC);

GO;
CREATE INDEX IX_PasswordResetCodes_Active
    ON dbo.PasswordResetCodes (UserId, Used, ExpiresAt);
