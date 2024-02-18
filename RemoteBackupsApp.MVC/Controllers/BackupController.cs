using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RemoteBackupsApp.Domain.ViewModels.Backup;
using RemoteBackupsApp.Infrastructure.Attributes;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using RemoteBackupsApp.MVC.Models;
using System.Diagnostics;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;
        private readonly IMemoryCache _memoryCache;
        private readonly IUserContext _userContext;
        public BackupController(IBackupService backupService, IMemoryCache memoryCache, IUserContext userContext)
        {
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<ActionResult> Index()
        {
            var backups = await _backupService.GetBackups();

            if (!_memoryCache.TryGetValue("BackupsIndex", out IEnumerable<BackupViewModel> cachedData))
            {
                cachedData = backups;
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                _memoryCache.Set("BackupsIndex", cachedData, cacheEntryOptions);
            }

            return View(backups);
        }

        public ActionResult Create()
            => PartialView("~/Views/Backup/_CreateBackupModal.cshtml");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormFile file)
        {
            try
            {
                await _backupService.CreateBackup(file);

                _memoryCache.Remove("BackupsIndex");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public async Task<ActionResult> Download(string backupId)
        {
            var uploadFileViewModel = await _backupService.DownloadBackup(backupId);

            return File(uploadFileViewModel.EncryptedData, uploadFileViewModel.ContentType, fileDownloadName: uploadFileViewModel.BackupName);
        }

        public async Task<ActionResult> Delete(string backupId)
        {
            await _backupService.DeleteBackup(backupId);

            _memoryCache.Remove("BackupsIndex");

            return RedirectToAction(nameof(Index));
        }
    }
}
