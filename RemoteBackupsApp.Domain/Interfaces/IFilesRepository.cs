using RemoteBackupsApp.Domain.Models;

namespace RemoteBackupsApp.Domain.Interfaces
{
    public interface IFilesRepository
    {
        public Task<IEnumerable<FileDto>>GetFiles(int userId, int pageNumber, int pageSize,
                string sortColumn = "CreatedAt", string sortDirection = "desc");
        public Task<int> SaveFile(FileDto file);
        public Task<string> GetFilePath(int fileId);
        public Task<int> Delete(int fileId, int userId);
        public Task<int> GetFilesCount(int userId);
        public Task<int> CheckFileUploadStatus(int fileId);
    }
}
