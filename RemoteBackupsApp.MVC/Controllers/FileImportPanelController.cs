using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using RemoteBackupsApp.MVC.Models.PageResult;

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
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string sortColumn = "CreatedAt", string sortDirection = "desc")
        {
            var userId = _memoryCache.Get<int>("UserId");

            var uploads = await _fileUploadProcessRepository.GetByUserId(userId, pageNumber, pageSize, sortColumn, sortDirection);

            var totalCount = await _fileUploadProcessRepository.GetFilesUploadsCount(userId);

            var model = new PagedResult<FileUploadProgress>
            {
                Items = uploads,
                PageNumber = pageNumber,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(model);
        }
    }
}
