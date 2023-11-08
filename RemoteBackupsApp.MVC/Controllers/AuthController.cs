using Microsoft.AspNetCore.Mvc;
using RemoteBackupsApp.Domain.ViewModels.Authentication;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public IActionResult Login()
            => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            var loginResponse = await _authenticationService.Login(loginViewModel);

            //Obsluga przypadkow

            return RedirectToAction("Index", "Backup");
        }

        public IActionResult Register()
            => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            var registerResponse = await _authenticationService.Register(registerViewModel);

            if (registerResponse)
            {
                await _authenticationService.Login(new LoginViewModel()
                {
                    UserName = registerViewModel.UserName,
                    Password = registerViewModel.Password
                });

                return RedirectToAction("Index", "Backup"); ;
            }

            return View();
        }

        public async Task<IActionResult> LogOut(string userName)
        {
            await _authenticationService.LogOut(userName);

            return RedirectToAction("Index", "Backup");
        }
    }
}
