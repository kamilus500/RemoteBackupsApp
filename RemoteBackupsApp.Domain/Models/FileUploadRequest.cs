namespace RemoteBackupsApp.Domain.Models
{
    public class FileUploadRequest
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string TargetFolder { get; set; } = "UploadedFiles";
        public string UserId { get; set; }
        public string FileExtension { get; set; }
        public double FileSize { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
