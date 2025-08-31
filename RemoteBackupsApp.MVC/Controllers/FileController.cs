using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NToastNotify;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using RemoteBackupsApp.Infrastructure.Helpers;
using RemoteBackupsApp.Infrastructure.Repositories;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class FileController : Controller
    {
        private readonly IFilesRepository _fileRepository;
        private readonly IFileQueueService _fileQueue;
        private readonly IFileUploadProcessRepository _fileUploadProcessRepository;
        private readonly IToastNotification _toastNotification;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _env;
        public FileController(IFilesRepository filesRepository, IFileQueueService fileQueue, IFileUploadProcessRepository fileUploadProcessRepository, IToastNotification toastNotification, IWebHostEnvironment env, IMemoryCache memoryCache)
        {
            _fileRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
            _fileQueue = fileQueue ?? throw new ArgumentNullException(nameof(fileQueue));
            _fileUploadProcessRepository = fileUploadProcessRepository ?? throw new ArgumentNullException(nameof(fileUploadProcessRepository));
            _toastNotification = toastNotification ?? throw new ArgumentNullException(nameof(toastNotification));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _env = env ?? throw new ArgumentNullException(nameof(env));
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


            var userId = _memoryCache.Get<int>("UserId");

            var filePath = Path.Combine("UserFiles", _memoryCache.Get<int>("UserId").ToString(), file.FileName);
            var webRootPath = Path.Combine(_env.WebRootPath, "uploads") + $"\\{userId}";
            var targetPath = Path.Combine(webRootPath, file.FileName);

            var request = new FileUploadRequest
            {
                File = await FileHelper.ConvertToBytesAsync(file),
                FileName = file.FileName,
                FileSize = file.Length,
                FileExtension = Path.GetExtension(file.FileName),
                FilePath = targetPath,
                UserId = userId
            };

            var newFile = new FileDto()
            {
                FileSize = file.Length,
                FileExtension = Path.GetExtension(file.FileName),
                FileName = file.FileName,
                FilePath = targetPath,
                UserId = userId
            };

            var fileId = await _fileRepository.SaveFile(newFile);

            var processId = await _fileUploadProcessRepository.Create(userId, fileId);

            request.ProcessId = processId;

            await _fileQueue.EnqueueFileAsync(request);

            _toastNotification.AddInfoToastMessage("Ddano plik do kolejki przesyłania.");

            return RedirectToAction("Index");
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

            await _fileRepository.Delete(fileId, userId);

            var filePath = await _fileRepository.GetFilePath(fileId);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                _toastNotification.AddAlertToastMessage("Nie znaleziono pliku.");
                return RedirectToAction("Index");
            }

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            else
            {
                _toastNotification.AddAlertToastMessage(
                    "Plik został usunięty z bazy danych, ale nie znaleziono go na dysku."
                );
            }

            return RedirectToAction("Index");
        }

    }
}
