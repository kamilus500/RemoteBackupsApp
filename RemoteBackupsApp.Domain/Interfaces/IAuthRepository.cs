namespace RemoteBackupsApp.Domain.Interfaces
{
    public interface IAuthRepository
    {
        Task<int> LoginAsync(string userName, string password);
        Task<int> RegisterAsync(string userName, string email, string password);
        Task<int> Logout(string userName);
    }
}
