using RemoteBackupsApp.Domain.Models;

namespace RemoteBackupsApp.Domain.Interfaces
{
    public interface IAuthRepository
    {
        Task<LoginResult> LoginAsync(string userName, string password);
        Task<int> RegisterAsync(string userName, string email, string password);
        Task<int> Logout(string userName);
        Task<int> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    }
}
