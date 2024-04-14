using Dapper;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using RemoteBackupsApp.Domain.ViewModels.Backup;
using RemoteBackupsApp.Infrastructure.Hubs;
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
        private readonly IHubContext<NotyfyCompleteTaskHub> _hubContext;

        public FileProcessingService(DatabaseContext databaseContext, IFileService fileService, IEncryptionService encryptionService, IHubContext<NotyfyCompleteTaskHub> hubContext)
        {
            _dbContext = databaseContext.CreateConnection();
            _fileService = fileService;
            _encryptionService = encryptionService;
            _hubContext = hubContext;
        }

        public void ProcessFile(FileProcessViewModel fileProcessViewModel)
        {
            BackgroundJob.Enqueue(() => AddBackupToDatabase(fileProcessViewModel));
        }

        public void AddBackupToDatabase(FileProcessViewModel fileProcessViewModel)
        {
            using (var fileStream = File.Create(fileProcessViewModel.TempFilePath))
            using (_dbContext)
            {
                var fileSize = _fileService.ConvertFileSize(fileStream.Length);

                var encryptedData = _encryptionService.Encrypt(fileStream);

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

            _hubContext.Clients.All.SendAsync("JobCompleted");

            File.Delete(fileProcessViewModel.TempFilePath);
        }
    }
}
