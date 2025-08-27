IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BackupsDb')
BEGIN
    CREATE DATABASE BackupsDb;
END
GO

USE BackupsDb;
GO

-- Users
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Users') AND type = 'U')
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId       INT IDENTITY(1,1) PRIMARY KEY,
        Username     NVARCHAR(100) NOT NULL UNIQUE,
        Email        NVARCHAR(255) NOT NULL UNIQUE,
        IsLogged     BIT NOT NULL DEFAULT(0),
        PasswordHash VARBINARY(256) NOT NULL,
        CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- Files
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Files') AND type = 'U')
BEGIN
    CREATE TABLE dbo.Files
    (
        FileId        INT IDENTITY(1,1) PRIMARY KEY,
        UserId        INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        FileName      NVARCHAR(255) NOT NULL,
        FileExtension NVARCHAR(10) NULL,
        FileSize      BIGINT NOT NULL,
        FilePath      NVARCHAR(1000) NULL,
        CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

--Sessions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Sessions') AND type = 'U')
BEGIN
    CREATE TABLE dbo.Sessions
    (
        SessionId   UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        UserId      INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        LoginTime   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ExpireTime  DATETIME2 NOT NULL
    );
END
GO

-- FileAccessLog
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.FileAccessLog') AND type = 'U')
BEGIN
    CREATE TABLE dbo.FileAccessLog
    (
        LogId        INT IDENTITY(1,1) PRIMARY KEY,
        FileId       INT NOT NULL FOREIGN KEY REFERENCES dbo.Files(FileId),
        UserId       INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        Action       NVARCHAR(50) NOT NULL, -- np. 'DOWNLOAD', 'UPLOAD', 'UPDATE', 'DELETE'
        ActionTime   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

--Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Files_UserId' AND object_id = OBJECT_ID('dbo.Files'))
    CREATE INDEX IX_Files_UserId ON dbo.Files(UserId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sessions_UserId' AND object_id = OBJECT_ID('dbo.Sessions'))
    CREATE INDEX IX_Sessions_UserId ON dbo.Sessions(UserId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FileAccessLog_UserId' AND object_id = OBJECT_ID('dbo.FileAccessLog'))
    CREATE INDEX IX_FileAccessLog_UserId ON dbo.FileAccessLog(UserId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FileAccessLog_FileId' AND object_id = OBJECT_ID('dbo.FileAccessLog'))
    CREATE INDEX IX_FileAccessLog_FileId ON dbo.FileAccessLog(FileId);
GO

--(-99) - SqlError
--(0) - User with username or email exist
--(1) - Success
--Procedures
CREATE OR ALTER PROCEDURE dbo.CreateUser
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @PasswordHash VARBINARY(256)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username OR Email = @Email)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Result;
            RETURN;
        END

        INSERT INTO dbo.Users (Username, Email, PasswordHash)
        VALUES (@Username, @Email, @PasswordHash);

        COMMIT TRANSACTION;

        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        SELECT -99 AS Result;
    END CATCH
END
GO

--(-99) - SqlError
--(-1) - User is null
--(0) - User is logged
--(1) - Success
CREATE OR ALTER PROCEDURE dbo.LoginUser
    @Username NVARCHAR(100),
    @PasswordHash VARBINARY(256)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT, @AlreadyLogged BIT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @UserId = UserId, @AlreadyLogged = IsLogged
        FROM dbo.Users
        WHERE Username = @Username AND PasswordHash = @PasswordHash;

        IF @UserId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result;
        END

        IF @AlreadyLogged = 1
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Result;
        END

        UPDATE dbo.Users
        SET IsLogged = 1
        WHERE UserId = @UserId;

        INSERT INTO dbo.Sessions (UserId, ExpireTime)
        VALUES (@UserId, DATEADD(HOUR, 1, SYSUTCDATETIME()));

        COMMIT TRANSACTION;

        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();

        SELECT -99 AS Result;
    END CATCH
END
GO

--(-99) - SqlError
--(-1) - User is null
--(0) - User is logged
--(1) - Success
CREATE OR ALTER PROCEDURE dbo.LogoutUser
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT, @IsLogged BIT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @UserId = UserId, @IsLogged = IsLogged
        FROM dbo.Users
        WHERE Username = @Username;

        IF @UserId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result;
        END

        IF @IsLogged = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Result;
        END

        UPDATE dbo.Users
        SET IsLogged = 0
        WHERE UserId = @UserId;

        DELETE FROM dbo.Sessions
        WHERE UserId = @UserId;

        COMMIT TRANSACTION;

        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
        SELECT -99 AS Result;
    END CATCH
END
GO