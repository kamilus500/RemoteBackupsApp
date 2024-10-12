using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RemoteBackupsApp.Domain.ViewModels.Backup;
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
            if (!await _userContext.IsUserLogIn())
                return View(new List<BackupViewModel>());

            if (!_memoryCache.TryGetValue("BackupsIndex", out IEnumerable<BackupViewModel> cachedData))
            {
                var backups = await _backupService.GetBackups();
                cachedData = backups;
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                };
                _memoryCache.Set("BackupsIndex", cachedData, cacheEntryOptions);
            }

            return View(cachedData);
        }

        public ActionResult Create()
            => PartialView("~/Views/Backup/_CreateBackupModal.cshtml");

        [HttpPost]
        [RequestSizeLimit(1024*1024*10)]
        [ValidateAntiForgeryToken]
        public async Task Create(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var tempFilePath = Path.GetTempFileName();

                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var processFileViewModel = new FileProcessViewModel()
                    {
                        ContentType = file.ContentType,
                        FileName = file.FileName,
                        FileLength = (int)file.Length,
                        TempFilePath = tempFilePath
                    };

                    await _backupService.CreateBackup(processFileViewModel);
                }

                GC.Collect();

                ModelState.Clear();
            }
            catch
            {
                //return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
