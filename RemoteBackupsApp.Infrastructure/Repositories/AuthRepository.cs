using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
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

        public async Task<LoginResult> LoginAsync(string userName, string password)
        {
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            return await _sqlService.QuerySingleAsync<LoginResult>(
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

        public async Task<int> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var oldPasswordBytes = System.Text.Encoding.UTF8.GetBytes(oldPassword);
            var newPasswordBytes = System.Text.Encoding.UTF8.GetBytes(newPassword);

            return await _sqlService.QuerySingleAsync<int>(
                "dbo.ChangePassword",
                new { UserId = userId, OldPasswordHash = oldPasswordBytes, NewPasswordHash = newPasswordBytes },
                CommandType.StoredProcedure);
        }

    }
}
