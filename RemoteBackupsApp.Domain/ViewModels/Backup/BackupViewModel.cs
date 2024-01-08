namespace RemoteBackupsApp.Domain.ViewModels.Backup
{
    public class BackupViewModel
    {
        public Guid Id { get; set; }
        public string BackupName { get; set; }
        public DateTime CreationDate { get; set; }
        public string Size { get; set; }
    }
}
