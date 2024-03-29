﻿using Dapper;
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
                _dbContext.Execute(DbQueries.CreateRoleTableQuery);

                _dbContext.Execute(DbQueries.InsertRoleTableQuery);

                _dbContext.Execute(DbQueries.CreateUserTableQuery);

                _dbContext.Execute(DbQueries.CreateBackupTableQuery);

                _dbContext.Execute(DbQueries.CreateBackupProcedure);

                _dbContext.Execute(DbQueries.CreateNewUserProcedure);

                _dbContext.Execute(DbQueries.CreateLoginProcedure);

                _dbContext.Execute(DbQueries.CreateLogOutProcedure);

                _dbContext.Execute(DbQueries.CreateRemoveBackupProcedure);

                _dbContext.Execute(DbQueries.CreateBanUserProcedure);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
        }
    }
}
