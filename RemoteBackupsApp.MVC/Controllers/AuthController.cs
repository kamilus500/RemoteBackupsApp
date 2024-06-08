using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using RemoteBackupsApp.Domain.Enums;
using RemoteBackupsApp.Domain.ViewModels.Authentication;
using RemoteBackupsApp.Infrastructure.Attributes;
using RemoteBackupsApp.Infrastructure.Helpers;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEmailService _emailService;
        private readonly INotyfService _notyfService;
        private readonly IStringLocalizer<AuthController> _localizer;
        private readonly IUserContext _userContext;
        public AuthController(IAuthenticationService authenticationService, IEmailService emailService, INotyfService notyfService, IStringLocalizer<AuthController> localizer, IUserContext userContext)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _notyfService = notyfService ?? throw new ArgumentNullException(nameof(notyfService));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public IActionResult Login()
            => PartialView("~/Views/Auth/_Login.cshtml");

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            LoginResponseEnum loginResponse = await _authenticationService.Login(loginViewModel);

            switch (loginResponse)
            {
                case LoginResponseEnum.NotCorrectPassword:
                    _notyfService.Error(_localizer["NotCorrectPassword"].Value);
                    break;
                case LoginResponseEnum.Success:
                    _notyfService.Success(_localizer["SuccessLogin"].Value);
                    break;
                case LoginResponseEnum.Logged:
                    _notyfService.Error(_localizer["LoggedUser"].Value);
                    break;
                case LoginResponseEnum.Banned:
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

                _notyfService.Success("User was register succesfully");

                return RedirectToAction("Index", "Backup"); ;
            }

            return View();
        }

        public async Task<IActionResult> LogOut(string userName)
        {
            await _authenticationService.LogOut(userName);

            return RedirectToAction("Index", "Backup");
        }

        [AdminAuthorize]
        public async Task<IActionResult> AdminPanel()
            => View(await _userContext.GetAllUsers());

        [AdminAuthorize]
        public async Task<IActionResult> BanUser(string userName)
        {
            await _userContext.BanUser(userName);

            return RedirectToAction(nameof(AdminPanel));
        }
    }
}
