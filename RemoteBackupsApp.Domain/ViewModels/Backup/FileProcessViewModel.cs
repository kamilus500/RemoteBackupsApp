namespace RemoteBackupsApp.Domain.ViewModels.Backup
{
    public class FileProcessViewModel
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string UserId { get; set; }
        public string ContentType { get; set; }
        public int FileLength { get; set; }
    }
}
