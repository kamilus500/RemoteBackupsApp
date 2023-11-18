using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RemoteBackupsApp.Infrastructure.Helpers;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Initializers
{
    public class Seeder
    {
        private IDbConnection _dbContext;
        public Seeder(IConfiguration configuration)
        {
            _dbContext = new SqlConnection(configuration.GetConnectionString("conString"));
        }
        public int IsDatatabaseExist()
            => _dbContext.QueryFirstOrDefault<int>(DbQueries.IsDatabaseExistQuery);
   

        public void CreateDatabase()
        {
            try
            {
                _dbContext.Execute(DbQueries.DbCreateQuery);

                //Edit connectionString after create database
                _dbContext.ConnectionString += "Database=RemoteBackupDb";

                _dbContext.Execute(DbQueries.CreateUserTableQuery);

                _dbContext.Execute(DbQueries.CreateBackupTableQuery);

                _dbContext.Execute(DbQueries.CreateBackupProcedure);

                _dbContext.Execute(DbQueries.CreateNewUserProcedure);

                _dbContext.Execute(DbQueries.CreateLoginProcedure);

                _dbContext.Execute(DbQueries.CreateLogOutProcedure);

                _dbContext.Execute(DbQueries.CreateRemoveBackupProcedure);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
        }
    }
}
