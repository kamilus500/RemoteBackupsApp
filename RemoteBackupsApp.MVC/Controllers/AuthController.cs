using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NToastNotify;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.MVC.Models.Auth;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly IToastNotification _toastNotification;
        private readonly IMemoryCache _memoryCache;
        public AuthController(IAuthRepository authRepository, IToastNotification toastNotification, IMemoryCache memoryCache)
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
                return RedirectToAction("Index", "Home");
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRegisterViewModel viewModel)
        {
            var result = await _authRepository.LoginAsync(viewModel.Username, viewModel.Password);

            if (result == -99)
            {
                _toastNotification.AddErrorToastMessage("Coś poszło nie tak!");
                return RedirectToAction("Index");
            }

            if (result == -1)
            {
                _toastNotification.AddErrorToastMessage("Użytkownik nie istnieje!");
                return RedirectToAction("Index");
            }

            if (result == 0)
            {
                _toastNotification.AddErrorToastMessage("Użytkownik jest już zalogowany!");
                return RedirectToAction("Index");
            }

            if (result == 1)
            {
                _memoryCache.Set("Username", viewModel.Username);
                _memoryCache.Set("IsLogged", true);
                _toastNotification.AddSuccessToastMessage("Zalogowany!");
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
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
                    _memoryCache.Set("IsLogged", true);
                    _toastNotification.AddSuccessToastMessage("Zalogowany!");
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Logout(string userName)
        {
            if (ModelState.IsValid)
            {
                var result = await _authRepository.Logout(userName);

                if (result == 1)
                {
                    _memoryCache.Remove("IsLogged");
                }
            }

            return RedirectToAction("Index");
        }
    }
}
