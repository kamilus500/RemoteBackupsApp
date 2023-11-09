using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Initializers
{
    public class Seeder
    {
        private IDbConnection _dbContext;
        public Seeder(DatabaseContext databaseContext)
        {
            _dbContext = databaseContext.CreateConnection();
        }

        public int IsDatatabaseExist()
        {
            string isDatabaseExistQuery = @"IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RemoteBackupDb')
	                                                BEGIN
		                                                SELECT 1
	                                                END
                                                ELSE
	                                                BEGIN
		                                                SELECT 0
	                                                END";

            return _dbContext.QueryFirstOrDefault<int>(isDatabaseExistQuery);
        }

        public void CreateDatabase()
        {
            try
            {
                string dbCreateQuery = @"CREATE DATABASE RemoteBackupDb";

                _dbContext.Execute(dbCreateQuery);

                string useDbQuery = @"USE RemoteBackupDb";

                _dbContext.Execute(useDbQuery);

                string createUserTableQuery = @"CREATE TABLE UserTable(
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    Email NVARCHAR(30) NOT NULL,
                    UserName NVARCHAR(30) NOT NULL,
                    PasswordHashed VARBINARY(MAX) NOT NULL,
                    IsLogin BIT NOT NULL
                    )";

                _dbContext.Execute(createUserTableQuery);

                string createBackupTableQuery = @"CREATE TABLE BackupTable(
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

                _dbContext.Execute(createBackupTableQuery);

                string createBackupProcedure = @"CREATE OR ALTER PROCEDURE CreateBackup
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

                _dbContext.Execute(createBackupProcedure);

                string createNewUserProcedure = @"CREATE OR ALTER PROCEDURE CreateNewUser
                        @Email NVARCHAR(30),
                        @UserName NVARCHAR(30),
                        @Password NVARCHAR(20)
                    AS
                    BEGIN
                        INSERT INTO UserTable (Id, Email, UserName, PasswordHashed, IsLogin)
                        VALUES (NEWID(), @Email, @UserName, HASHBYTES('SHA2_512', @Password), 0);
                    END;";

                _dbContext.Execute(createNewUserProcedure);

                string createLoginProcedure = @"CREATE OR ALTER PROCEDURE LoginUser
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
                    END";

                _dbContext.Execute(createLoginProcedure);

                string createLogOutProcedure = @"CREATE OR ALTER PROCEDURE LogOut 
	                    @UserName NVARCHAR(30)
                    AS
                    BEGIN
	                    UPDATE UserTable
	                    SET IsLogin = 0
	                    WHERE UserName = @UserName
                    END";

                _dbContext.Execute(createLogOutProcedure);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
        }
    }
}
