using System.Data;

namespace RemoteBackupsApp.Domain.Interfaces
{
    public interface ISqlService
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
        Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
        Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text);
    }
}
