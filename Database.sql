CREATE DATABASE RemoteBackupDb;

USE RemoteBackupDb;

CREATE TABLE BackupTable(
Id UNIQUEIDENTIFIER PRIMARY KEY,
BackupName NVARCHAR(30) NOT NULL,
CreationDate DATETIME NOT NULL,
EncryptedData VARBINARY(MAX) NOT NULL,
ContentType NVARCHAR(50) NOT NULL,
Size DECIMAL NOT NULL,
AesKey VARBINARY(MAX) NOT NULL,
AesIv VARBINARY(MAX) NOT NULL,
IsDeleted BIT NOT NULL
) 

CREATE TABLE UserTable(
Id UNIQUEIDENTIFIER PRIMARY KEY,
Email NVARCHAR(30) NOT NULL,
UserName NVARCHAR(30) NOT NULL,
PasswordHashed VARBINARY(MAX) NOT NULL,
IsLogin BIT NOT NULL
)

--create procedure
CREATE OR ALTER PROCEDURE CreateBackup
    @BackupName NVARCHAR(30),
    @CreationDate DATETIME,
    @EncryptedData VARBINARY(MAX),
	@ContentType NVARCHAR(50),
	@Size DECIMAL,
    @AesKey VARBINARY(MAX),
    @AesIv VARBINARY(MAX)
AS
BEGIN
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO BackupTable (Id, BackupName, CreationDate, Size, EncryptedData, ContentType, AesKey, AesIv, IsDeleted)
    VALUES (@NewId, @BackupName, @CreationDate, @Size, @EncryptedData, @ContentType, @AesKey, @AesIv, 0);
END

CREATE OR ALTER PROCEDURE LoginUser
	@UserName NVARCHAR(30),
	@Password NVARCHAR(30)
AS
BEGIN
	DECLARE @StoredPasswordHash VARBINARY(MAX);
	DECLARE @IsLogin BIT;

    SELECT @StoredPasswordHash = PasswordHashed
    FROM UserTable
    WHERE UserName = @Username;

	SELECT @IsLogin = IsLogin
	FROM UserTable
	WHERE UserName = @UserName;

	IF @IsLogin = 1 
	BEGIN
		Print 'Uzytkownik jest juz zalogowany';
	END
	ELSE 
	BEGIN
		IF @StoredPasswordHash IS NOT NULL
		BEGIN

			IF @StoredPasswordHash = HASHBYTES('SHA2_512', @Password)
			BEGIN
				UPDATE UserTable
				SET IsLogin = 1
				WHERE UserName = @UserName;

				SELECT 1 
			END
			ELSE
			BEGIN
				SELECT 0;
			END
		END
		ELSE
		BEGIN
			SELECT -1;
		END
	END
END

CREATE OR ALTER PROCEDURE LogOut 
	@UserName NVARCHAR(30)
AS
BEGIN
	UPDATE UserTable
	SET IsLogin = 0
	WHERE UserName = @UserName
END

SELECT *
FROM dbo.UserTable

SELECT *
FROM dbo.BackupTable

SELECT EncryptedData, ContentType, BackupName
FROM BackupTable

