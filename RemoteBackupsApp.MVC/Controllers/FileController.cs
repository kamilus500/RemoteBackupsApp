using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NToastNotify;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using RemoteBackupsApp.Infrastructure.Helpers;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class FileController : Controller
    {
        private readonly IFilesRepository _fileRepository;
        private readonly IFileQueueService _fileQueue;
        private readonly IToastNotification _toastNotification;
        private readonly IMemoryCache _memoryCache;
        public FileController(IFilesRepository filesRepository, IFileQueueService fileQueue, IToastNotification toastNotification, IMemoryCache memoryCache)
        {
            _fileRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
            _fileQueue = fileQueue ?? throw new ArgumentNullException(nameof(fileQueue));
            _toastNotification = toastNotification ?? throw new ArgumentNullException(nameof(toastNotification));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _memoryCache.Get<int>("UserId");

            var files = await _fileRepository.GetFiles(userId);

            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                _toastNotification.AddAlertToastMessage("Plik nie został przesłany.");

            var jobId = Guid.NewGuid().ToString();

            var request = new FileUploadRequest
            {
                File = await FileHelper.ConvertToBytesAsync(file),
                FileName = file.FileName,
                FileSize = file.Length,
                CreatedAt = DateTime.UtcNow,
                FileExtension = Path.GetExtension(file.FileName),
                FilePath = Path.Combine("UserFiles", _memoryCache.Get<int>("UserId").ToString(), file.FileName),
                UserId = _memoryCache.Get<int>("UserId").ToString(),
                TargetFolder = Path.Combine("UserFiles", _memoryCache.Get<int>("UserId").ToString()),
                JobId = jobId
            };

            await _fileQueue.EnqueueFileAsync(request);

            _toastNotification.AddInfoToastMessage("Ddano plik do kolejki przesyłania.");

            return Ok(new { jobId });
        }

        [HttpGet]
        public async Task<IActionResult> Download(int fileId)
        {
            var filePath = await _fileRepository.GetFilePath(fileId);

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                _toastNotification.AddInfoToastMessage("Nie znaleziono pliku.");
                return RedirectToAction("Index");
            }

            var fileName = Path.GetFileName(filePath);
            var contentType = "application/octet-stream"; 

            return PhysicalFile(filePath, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int fileId)
        {
            var userId = _memoryCache.Get<int>("UserId");

            await _fileRepository.Delete(fileId,userId);

            return RedirectToAction("Index");
        }
    }
}
