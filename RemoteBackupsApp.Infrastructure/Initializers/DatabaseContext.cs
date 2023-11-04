using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Initializers
{
    public class DatabaseContext
    {
        protected readonly IConfiguration Configuration;

        public DatabaseContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString: Configuration.GetConnectionString("conString"));
        }
    }
}
