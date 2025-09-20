using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using NToastNotify;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Infrastructure.Hubs;
using RemoteBackupsApp.MVC.Models.Auth;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly IToastNotification _toastNotification;
        private readonly IMemoryCache _memoryCache;
        public AuthController(IAuthRepository authRepository, IToastNotification toastNotification, IMemoryCache memoryCache, IHubContext<UploadHub> hub)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
            _toastNotification = toastNotification ?? throw new ArgumentNullException(nameof(toastNotification));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_memoryCache.TryGetValue("IsLogged", out bool isLogged) && isLogged)
            {
                return RedirectToAction("Index", "File");
            }

            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRegisterViewModel viewModel)
        {
            var result = await _authRepository.LoginAsync(viewModel.Username, viewModel.Password);

            if (result.Result == -99)
            {
                _toastNotification.AddErrorToastMessage("Coś poszło nie tak!");
                return RedirectToAction("Index");
            }

            if (result.Result == -1)
            {
                _toastNotification.AddErrorToastMessage("Złe hasło!");
                return RedirectToAction("Index");
            }

            if (result.Result == 0)
            {
                _toastNotification.AddErrorToastMessage("Użytkownik jest już zalogowany!");
                return RedirectToAction("Index");
            }

            if (result.Result == 1)
            {
                _memoryCache.Set("Username", viewModel.Username);
                _memoryCache.Set("UserId", result.UserId);
                _memoryCache.Set("IsLogged", true);

                return RedirectToAction("Index", "File");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(LoginRegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _authRepository.RegisterAsync(viewModel.Username, viewModel.Email, viewModel.Password);

                if (result == -99)
                {
                    _toastNotification.AddErrorToastMessage("Coś poszło nie tak!");
                    return RedirectToAction("Index");
                }

                if (result == 0)
                {
                    _toastNotification.AddErrorToastMessage("Użytkownik o takiej nazwie lub emailu już istnieje");
                    return RedirectToAction("Index");
                }

                if (result == 1)
                {
                    _toastNotification.AddSuccessToastMessage("Zarejestrowany!");
                    return RedirectToAction("Index", "Auth");
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string userName)
        {
            if (ModelState.IsValid)
            {
                var result = await _authRepository.Logout(userName);

                if (result == 1)
                {
                    _memoryCache.Remove("IsLogged");
                    _memoryCache.Remove("Username");
                    _memoryCache.Remove("UserId");
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _memoryCache.Get<int>("UserId");

                var result = await _authRepository.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

                if (result == -99)
                {
                    _toastNotification.AddErrorToastMessage("Coś poszło nie tak!");
                    return RedirectToAction("ChangePassword", "Auth");
                }

                if (result == -1)
                {
                    _toastNotification.AddErrorToastMessage("Hasła sie nie zgadzaja");
                    return RedirectToAction("ChangePassword", "Auth");
                }

                if (result == 1)
                {
                    _toastNotification.AddSuccessToastMessage("Hasło zostało zmienione pomyślnie. Zaloguj się ponownie.");
                    return RedirectToAction("Index", "File");
                }
            }

            return RedirectToAction("ChangePassword", "File");
        }
    }
}
