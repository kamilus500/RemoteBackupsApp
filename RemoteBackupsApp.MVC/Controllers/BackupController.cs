using Microsoft.AspNetCore.Mvc;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using RemoteBackupsApp.MVC.Models;
using System.Diagnostics;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;
        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        public async Task<ActionResult> Index()
        {
            var backups = await _backupService.GetBackups();

            return View(backups);
        }

        public ActionResult Create()
        {

            return PartialView("~/Views/Backup/_CreateBackupModal.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormFile file)
        {
            try
            {
                await _backupService.CreateBackup(file);
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

            return RedirectToAction(nameof(Index));
        }
    }
}
