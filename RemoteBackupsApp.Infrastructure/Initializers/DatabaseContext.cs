using Microsoft.Data.SqlClient;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Initializers
{
    public class DatabaseContext
    {
        private string _connectionString;

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString: _connectionString);
        }
    }
}
