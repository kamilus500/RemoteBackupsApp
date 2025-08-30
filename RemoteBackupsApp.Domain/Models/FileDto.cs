using Microsoft.AspNetCore.Http;

namespace RemoteBackupsApp.Domain.Models
{
    public class FileDto
    {
        public string FileId { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public IFormFile File { get; set; }
    }
}
