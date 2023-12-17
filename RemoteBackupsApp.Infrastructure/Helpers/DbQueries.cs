namespace RemoteBackupsApp.Infrastructure.Helpers
{
    public static class DbQueries
    {
        public static string DbCreateQuery = @"CREATE DATABASE RemoteBackupDb";

        public static string CreateRoleTableQuery = @"CREATE TABLE RoleTable(
                    Id INT PRIMARY KEY IDENTITY,
                    Name varchar(5)
                    )";

        public static string InsertRoleTableQuery = @"
                    INSERT INTO RoleTable (Name) Values ('User')
                    INSERT INTO RoleTable (Name) Values ('Admin')";

        public static string CreateUserTableQuery = @"CREATE TABLE UserTable(
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    Email NVARCHAR(30) NOT NULL,
                    UserName NVARCHAR(30) NOT NULL,
                    PasswordHashed VARBINARY(MAX) NOT NULL,
                    IsLogin BIT NOT NULL,
                    RoleId INT,
                    FOREIGN KEY (RoleId) REFERENCES RoleTable(Id)
                    )";

        public static string CreateBackupTableQuery = @"CREATE TABLE BackupTable(
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    BackupName NVARCHAR(30) NOT NULL,
                    CreationDate DATETIME NOT NULL,
                    EncryptedData VARBINARY(MAX) NOT NULL,
                    ContentType NVARCHAR(50) NOT NULL,
                    Size DECIMAL NOT NULL,
                    AesKey VARBINARY(MAX) NOT NULL,
                    AesIv VARBINARY(MAX) NOT NULL,
                    IsDeleted BIT NOT NULL,
                    UserId UNIQUEIDENTIFIER NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES UserTable(Id)
                    )";

        public static string CreateBackupProcedure = @"CREATE OR ALTER PROCEDURE CreateBackup
                        @BackupName NVARCHAR(30),
                        @CreationDate DATETIME,
                        @EncryptedData VARBINARY(MAX),
	                    @ContentType NVARCHAR(50),
	                    @Size DECIMAL,
                        @AesKey VARBINARY(MAX),
                        @AesIv VARBINARY(MAX),
	                    @UserId UNIQUEIDENTIFIER
                    AS
                    BEGIN
                        DECLARE @NewId UNIQUEIDENTIFIER = NEWID();

                        INSERT INTO BackupTable (Id, BackupName, CreationDate, Size, EncryptedData, ContentType, AesKey, AesIv, IsDeleted, UserId)
                        VALUES (@NewId, @BackupName, @CreationDate, @Size, @EncryptedData, @ContentType, @AesKey, @AesIv, 0, @UserId);
                    END";

        public static string CreateNewUserProcedure = @"CREATE OR ALTER PROCEDURE CreateNewUser
                        @Email NVARCHAR(30),
                        @UserName NVARCHAR(30),
                        @Password NVARCHAR(20)
                    AS
                    BEGIN
                        INSERT INTO UserTable (Id, Email, UserName, PasswordHashed, IsLogin)
                        VALUES (NEWID(), @Email, @UserName, HASHBYTES('SHA2_512', @Password), 0);
                    END;";

        public static string CreateLoginProcedure = @"CREATE OR ALTER PROCEDURE LoginUser
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
		                                                        RETURN 2;
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

				                                                        Print 'Uzytkownik zalogowany pomyslnie';
				                                                        RETURN 1;
			                                                        END
			                                                        ELSE
			                                                        BEGIN
				                                                        RETURN 0;
			                                                        END
		                                                        END
	                                                        END
                                                        END";

        public static string CreateLogOutProcedure = @"CREATE OR ALTER PROCEDURE LogOut 
	                    @UserName NVARCHAR(30)
                    AS
                    BEGIN
	                    UPDATE UserTable
	                    SET IsLogin = 0
	                    WHERE UserName = @UserName
                    END";

        public static string IsDatabaseExistQuery = @"IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RemoteBackupDb')
	                                                BEGIN
		                                                SELECT 1
	                                                END
                                                ELSE
	                                                BEGIN
		                                                SELECT 0
	                                                END";

        public static string CreateRemoveBackupProcedure = @"CREATE OR ALTER PROCEDURE RemoveBackup 
	                                                            @BackupId UNIQUEIDENTIFIER
                                                            AS
                                                            BEGIN
	                                                            UPDATE BackupTable
	                                                            SET IsDeleted = 1
	                                                            WHERE Id = @BackupId
                                                            END";
    }
}
