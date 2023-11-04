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
AesIv VARBINARY(MAX) NOT NULL
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

    INSERT INTO BackupTable (Id, BackupName, CreationDate, Size, EncryptedData, ContentType, AesKey, AesIv)
    VALUES (@NewId, @BackupName, @CreationDate, @Size, @EncryptedData, @ContentType, @AesKey, @AesIv);
END

SELECT *
FROM dbo.BackupTable

SELECT EncryptedData, ContentType, BackupName
FROM BackupTable