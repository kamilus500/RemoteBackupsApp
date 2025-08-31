namespace RemoteBackupsApp.Domain.Models
{
    public class FileUploadRequest
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public int UserId { get; set; }
        public string FileExtension { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }
        public int ProcessId { get; set; }
    }

}
