using RemoteBackupsApp.Domain.Models;

namespace RemoteBackupsApp.Domain.Interfaces
{
    public interface IFileUploadProcessRepository
    {
        Task<int> Create(int userId, int fileId);
        Task UpdateProgress(int progressId, decimal progressPct, string status);
        Task<IEnumerable<FileUploadProgress>> GetByUserId(int userId, int pageNumber, int pageSize, string sortColumn = "", string sortDirection = "desc");
        Task<int> GetFilesUploadsCount(int userId);
    }
}
