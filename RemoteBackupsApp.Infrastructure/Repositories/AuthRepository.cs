using RemoteBackupsApp.Domain.Interfaces;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ISqlService _sqlService;

        public AuthRepository(ISqlService sqlService)
        {
            _sqlService = sqlService;
        }

        public async Task<int> LoginAsync(string userName, string password)
        {
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            return await _sqlService.QuerySingleAsync<int>(
                "dbo.LoginUser",
                new { Username = userName, PasswordHash = passwordBytes },
                CommandType.StoredProcedure);
        }

        public async Task<int> Logout(string userName)
            => await _sqlService.QuerySingleAsync<int>(
                "dbo.LogoutUser",
                new { Username = userName },
                CommandType.StoredProcedure);

        public async Task<int> RegisterAsync(string userName, string email, string password)
        {
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            return await _sqlService.QuerySingleAsync<int>(
                "dbo.CreateUser",
                new { Username = userName, Email = email, PasswordHash = passwordBytes },
                CommandType.StoredProcedure);
        }
    }
}
