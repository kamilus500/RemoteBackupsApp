using Microsoft.AspNetCore.Http;

namespace RemoteBackupsApp.Domain.ViewModels
{
    public class CreateBackupViewModel
    {
        public IFormFile File { get; set; }
    }
}
