namespace RemoteBackupsApp.Domain.ViewModels
{
    public class BackupViewModel
    {
        public Guid Id { get; set; }
        public string BackupName { get; set; }
        public DateTime CreationDate { get; set; }
        public double Size { get; set; }
    }
}
