namespace RemoteBackupsApp.Domain.ViewModels
{
    public class EncryptionViewModel
    {
        public string BackupName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public byte[] AesKey { get; set; }
        public byte[] AesIv { get; set; }
    }
}
