using Dapper;
using Hangfire;
using RemoteBackupsApp.Domain.ViewModels.Backup;
using RemoteBackupsApp.Infrastructure.Initializers;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IDbConnection _dbContext;
        private readonly IFileService _fileService;
        private readonly IEncryptionService _encryptionService;

        public FileProcessingService(DatabaseContext databaseContext, IFileService fileService, IEncryptionService encryptionService)
        {
            _dbContext = databaseContext.CreateConnection();
            _fileService = fileService;
            _encryptionService = encryptionService;
        }

        public void ProcessFile(FileProcessViewModel fileProcessViewModel)
        {
            BackgroundJob.Enqueue(() => AddBackupToDatabase(fileProcessViewModel));
        }

        public void AddBackupToDatabase(FileProcessViewModel fileProcessViewModel)
        {
            var encryptedData = _encryptionService.Encrypt(fileProcessViewModel.Content);

            string fileSize = _fileService.ConvertFileSize(fileProcessViewModel.FileLength);

            var parameters = new
            {
                BackupName = $"{fileProcessViewModel.FileName}",
                CreationDate = DateTime.Now,
                EncryptedData = encryptedData.Content,
                ContentType = fileProcessViewModel.ContentType,
                AesKey = encryptedData.AesKey,
                AesIv = encryptedData.AesIv,
                Size = fileSize,
                UserId = Guid.Parse(fileProcessViewModel.UserId)
            };

            _dbContext.Execute("CreateBackup", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
