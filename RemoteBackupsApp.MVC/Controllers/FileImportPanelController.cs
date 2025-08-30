using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RemoteBackupsApp.Domain.Interfaces;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class FileImportPanelController : Controller
    {
        private readonly IFileUploadProcessRepository _fileUploadProcessRepository;
        private readonly IMemoryCache _memoryCache;
        public FileImportPanelController(IFileUploadProcessRepository fileUploadProcessRepository, IMemoryCache memoryCache)
        {
            _fileUploadProcessRepository = fileUploadProcessRepository ?? throw new ArgumentNullException(nameof(fileUploadProcessRepository));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _memoryCache.Get<int>("UserId");

            var uploads = await _fileUploadProcessRepository.GetByUserId(userId);

            return View(uploads);
        }
    }
}
