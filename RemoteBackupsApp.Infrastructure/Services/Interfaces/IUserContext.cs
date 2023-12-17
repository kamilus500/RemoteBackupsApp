using RemoteBackupsApp.Domain.ViewModels.User;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IUserContext
    {
        public Task<UserViewModel> GetUser();
        public Task<bool> IsUserLogIn();
        public Task<bool> IsInRole(string roleName);
    }
}
