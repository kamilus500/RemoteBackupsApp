using Microsoft.AspNetCore.Http;
using RemoteBackupsApp.Domain.ViewModels;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IBackupService
    {
        public Task<IEnumerable<BackupViewModel>> GetBackups();
        
        public Task<BackupViewModel> GetBackup(string backupId);

        public Task CreateBackup(IFormFile file);
        
        public Task<UploadBackupFileViewModel> DownloadBackup(string id);
    }
}
