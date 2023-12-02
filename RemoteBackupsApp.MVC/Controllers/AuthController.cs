using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using RemoteBackupsApp.Domain.ViewModels.Authentication;
using RemoteBackupsApp.Infrastructure.Helpers;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;
        private readonly INotyfService _notyfService;
        private readonly IStringLocalizer<AuthController> _localizer;
        public AuthController(IAuthenticationService authenticationService, IEmailService emailService, IMemoryCache memoryCache, INotyfService notyfService, IStringLocalizer<AuthController> localizer)
        {
            _authenticationService = authenticationService;
            _emailService = emailService;
            _memoryCache = memoryCache;
            _notyfService = notyfService;
            _localizer = localizer;
        }

        public IActionResult Login()
            => PartialView("~/Views/Auth/_Login.cshtml");

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            int loginResponse = await _authenticationService.Login(loginViewModel);

            if (!_memoryCache.TryGetValue("LoginResponse", out int cachedData))
            {
                cachedData = loginResponse;
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _memoryCache.Set("LoginResponse", cachedData, cacheEntryOptions);
            }

            switch (loginResponse)
            {
                case 0:
                    _notyfService.Error(_localizer["NotCorrectPassword"].Value);
                    break;
                case 1:
                    _notyfService.Success(_localizer["SuccessLogin"].Value);
                    break;
                case 2:
                    _notyfService.Error(_localizer["LoggedUser"].Value);
                    break;
                case 3:
                    _notyfService.Error(_localizer["NotExistUser"].Value);
                    break;
                default:
                    throw new ArgumentNullException(_localizer["SomethingWrong"].Value);
            }

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

                _notyfService.Error("User was register succesfully");

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
