using RemoteBackupsApp.Domain.Enums;
using RemoteBackupsApp.Domain.ViewModels.Authentication;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponseEnum> Login(LoginViewModel loginViewModel);
        Task<bool> Register(RegisterViewModel registerViewModel);
        Task LogOut(string userName);
    }
}
