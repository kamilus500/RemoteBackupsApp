using Microsoft.AspNetCore.Http;

namespace RemoteBackupsApp.Domain.ViewModels.Backup
{
    public class CreateBackupViewModel
    {
        public IFormFile File { get; set; }
    }
}
