﻿namespace RemoteBackupsApp.Domain.ViewModels.Backup
{
    public class UploadBackupFileViewModel
    {
        public string BackupName { get; set; }
        public byte[] EncryptedData { get; set; }
        public string ContentType { get; set; }
    }
}
