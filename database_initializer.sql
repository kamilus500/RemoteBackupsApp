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
        CreatedAt    DATETIME NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted    BIT NOT NULL DEFAULT(0)
    );
END
GO

-- Files
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Files') AND type = 'U')
BEGIN
    CREATE TABLE dbo.Files
    (
        FileId        INT IDENTITY(1,1) PRIMARY KEY,
        UserId        INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        FileName      NVARCHAR(255) NOT NULL,
        FileExtension NVARCHAR(10) NULL,
        FileSize      BIGINT NOT NULL,
        FilePath      NVARCHAR(1000) NULL,
        CreatedAt     DATETIME NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted    BIT NOT NULL DEFAULT(0)
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
        Action       NVARCHAR(50) NOT NULL, -- 'DOWNLOAD', 'UPLOAD', 'UPDATE', 'DELETE'
        ActionTime   DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- FileUploadProgress
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.FileUploadProgress') AND type = 'U')
BEGIN
    CREATE TABLE dbo.FileUploadProgress
    (
        ProgressId    INT IDENTITY(1,1) PRIMARY KEY,
        FileId        INT NOT NULL FOREIGN KEY REFERENCES dbo.Files(FileId),
        UserId        INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId),
        ProgressPct   DECIMAL(5,2) NOT NULL DEFAULT 0,
        Status        NVARCHAR(50) NOT NULL DEFAULT N'Pending', -- np. Pending, Uploading, Completed, Failed
        StartedAt     DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt     DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CompletedAt   DATETIME NULL
    );
END
GO

--Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Files_UserId' AND object_id = OBJECT_ID('dbo.Files'))
    CREATE INDEX IX_Files_UserId ON dbo.Files(UserId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FileAccessLog_UserId' AND object_id = OBJECT_ID('dbo.FileAccessLog'))
    CREATE INDEX IX_FileAccessLog_UserId ON dbo.FileAccessLog(UserId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FileAccessLog_FileId' AND object_id = OBJECT_ID('dbo.FileAccessLog'))
    CREATE INDEX IX_FileAccessLog_FileId ON dbo.FileAccessLog(FileId);
GO

--Procedures
--(-99) - SqlError
--(0) - User with username or email exist
--(1) - Success
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

--(-99) - Error
--(-1) - User not found
--(1) Success
CREATE OR ALTER PROCEDURE dbo.DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Users
        SET IsDeleted = 1
        WHERE UserId = @UserId
          AND IsDeleted = 0;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result
            RETURN;
        END

        COMMIT TRANSACTION;

        SELECT 1 AS Result
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();

        SELECT -99 AS Result;
        PRINT @ErrMsg;
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

        COMMIT TRANSACTION;

        SELECT 1 AS Result, @UserId AS UserId;
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

--(-99) - SqlError
--(1)   - Success
CREATE OR ALTER PROCEDURE dbo.InsertFile
    @UserId INT,
    @FileName NVARCHAR(255),
    @FileExtension NVARCHAR(10),
    @FileSize BIGINT,
    @FilePath NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FileId INT;

    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO dbo.Files (UserId, FileName, FileExtension, FileSize, FilePath)
        VALUES (@UserId, @FileName, @FileExtension, @FileSize, @FilePath);

        SET @FileId = SCOPE_IDENTITY();
        
        INSERT INTO dbo.FileAccessLog (FileId, UserId, Action)
        VALUES (@FileId, @UserId, 'UPLOAD');

        COMMIT TRANSACTION;

        SELECT @FileId AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();

        SELECT -99 AS Result;
        PRINT @ErrMsg;
    END CATCH
END
GO

--(-99) error
--(-1) User is null
--(1) Success
CREATE OR ALTER PROCEDURE dbo.DeleteFile
    @FileId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Files
        SET IsDeleted = 1
        WHERE FileId = @FileId
          AND IsDeleted = 0;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result
            RETURN;
        END

         INSERT INTO dbo.FileAccessLog (FileId, UserId, Action)
         VALUES (@FileId, @UserId, 'DELETE');

        COMMIT TRANSACTION;

        SELECT 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();

        SELECT -99 AS Result
        PRINT @ErrMsg;
    END CATCH
END
GO

--(-99) sql error
--(-2) user not exist
--(-1) file not exist
--(1) Success
CREATE OR ALTER PROCEDURE dbo.CreateFileUploadRequest
    @FileId   INT,
    @UserId   INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Files WHERE FileId = @FileId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @UserId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -2 AS Result;
            RETURN;
        END

        INSERT INTO dbo.FileUploadProgress (FileId, UserId)
        VALUES (@FileId, @UserId);

        COMMIT TRANSACTION;

        SELECT SCOPE_IDENTITY() AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        SELECT -99 AS Result;
        PRINT(@ErrMsg);
    END CATCH
END
GO

--(-99) -sql error
--(-1) - Fup is not exist
--(1) - Success
CREATE OR ALTER PROCEDURE dbo.UpdateFileUploadRequest
    @ProgressId   INT,
    @ProgressPct  DECIMAL(5,2),
    @Status       NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.FileUploadProgress WHERE ProgressId = @ProgressId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result;
            RETURN;
        END

        UPDATE dbo.FileUploadProgress
        SET ProgressPct = @ProgressPct,
            Status = @Status,
            UpdatedAt = GETUTCDATE(),
            CompletedAt = CASE WHEN @Status = N'Completed' THEN GETUTCDATE() ELSE NULL END
        WHERE ProgressId = @ProgressId;

        COMMIT TRANSACTION;

        SELECT 1 AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        SELECT -99 AS Result;
        PRINT(@ErrMsg);
    END CATCH
END
GO

--(-99) SqlError
--(-1) not same passwords
--(1) success
CREATE OR ALTER PROCEDURE dbo.ChangePassword
    @UserId       INT,
    @OldPasswordHash VARBINARY(256),
    @NewPasswordHash VARBINARY(256)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ExistingUserId  INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @ExistingUserId = UserId
        FROM dbo.Users
        WHERE UserId = @UserId
          AND PasswordHash = @OldPasswordHash
          AND IsDeleted = 0;

        IF @ExistingUserId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT -1 AS Result;
            RETURN;
        END

        UPDATE dbo.Users
        SET PasswordHash = @NewPasswordHash
        WHERE UserId = @UserId;

        COMMIT TRANSACTION;

        SELECT 1 AS Result
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


--Views
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'dbo.vwUserFiles'))
BEGIN
    EXEC('
        CREATE VIEW dbo.vwUserFiles
        AS
        SELECT
            UserId,
            FileId,
            FileExtension,
            FileName,
            FileSize,
            CreatedAt,
            IsDeleted
        FROM dbo.Files
    ')
END
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'dbo.vwFileUploadProgress'))
BEGIN
    EXEC('
        CREATE VIEW dbo.vwFileUploadProgress
        AS
        SELECT
            fup.ProgressId,
            fup.FileId,
            f.FileName,
            f.FileExtension,
            f.FileSize,
            fup.UserId,
            fup.ProgressPct,
            fup.Status,
            fup.StartedAt,
            fup.UpdatedAt,
            fup.CompletedAt,
            f.CreatedAt
        FROM dbo.FileUploadProgress fup
        INNER JOIN dbo.Files f ON fup.FileId = f.FileId
    ')
END
GO