using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RemoteBackupsApp.Domain.Interfaces;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class SqlService : ISqlService
    {
        private readonly string _connectionString;

        public SqlService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connectionString")
                               ?? throw new InvalidOperationException("Connection string is null or empty");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<T>(sql, parameters, commandType: commandType);
        }

        public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandType: commandType);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: commandType);
        }
    }
}
