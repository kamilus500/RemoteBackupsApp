using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RemoteBackupsApp.Domain.ViewModels.Authentication;
using RemoteBackupsApp.Domain.ViewModels.Backup;
using RemoteBackupsApp.Infrastructure.Helpers;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;
        public AuthController(IAuthenticationService authenticationService, IEmailService emailService, IMemoryCache memoryCache)
        {
            _authenticationService = authenticationService;
            _emailService = emailService;
            _memoryCache = memoryCache;
        }

        public IActionResult Login()
            => PartialView("~/Views/Auth/_Login.cshtml");

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            var loginResponse = await _authenticationService.Login(loginViewModel);

            if (!_memoryCache.TryGetValue("LoginResponse", out int cachedData))
            {
                cachedData = loginResponse;
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _memoryCache.Set("LoginResponse", cachedData, cacheEntryOptions);
            }

            //Obsluga przypadkow

            return RedirectToAction("Index", "Backup");
        }

        public IActionResult Register()
            => PartialView("~/Views/Auth/_Register.cshtml");

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            var registerResponse = await _authenticationService.Register(registerViewModel);

            await _emailService.Send(registerViewModel.Email, "Pomyślna rejestracja", EmailContents.ConfirmationMessage);

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

            _memoryCache.Remove("LoginResponse");

            return RedirectToAction("Index", "Backup");
        }
    }
}
