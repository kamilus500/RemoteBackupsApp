using Dapper;
using Microsoft.AspNetCore.Http;
using RemoteBackupsApp.Domain.ViewModels.Backup;
using RemoteBackupsApp.Domain.ViewModels.Encryption;
using RemoteBackupsApp.Infrastructure.Initializers;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private readonly IDbConnection _dbContext;
        private readonly IFileService _fileService;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IEncryptionService _encryptionService;
        private readonly IUserContext _userContext;

        public BackupService(DatabaseContext databaseContext, 
            IFileService fileService, 
            IEncryptionService encryptionService,
            IUserContext userContext,
            IFileProcessingService fileProcessingService
        )
        {
            _fileService = fileService;
            _dbContext = databaseContext.CreateConnection();
            _encryptionService = encryptionService;
            _userContext = userContext;
            _fileProcessingService = fileProcessingService;
        }

        public async Task CreateBackup(IFormFile file)
        {
            var user = await _userContext.GetUser();

            var content = await _fileService.GetFileContent(file);

            var fileProcessViewModel = new FileProcessViewModel
            {
                FileName = file.FileName,
                FileLength = content.Length,
                ContentType = file.ContentType,
                UserId = user.Id,
                Content = content
            };

            _fileProcessingService.ProcessFile(fileProcessViewModel);
        }

        public async Task DeleteBackup(string backupId)
        {
            var parameter = new
            {
                BackupId = backupId
            };

            await _dbContext.ExecuteAsync("RemoveBackup", parameter, commandType: CommandType.StoredProcedure);
        }

        public async Task<UploadBackupFileViewModel> DownloadBackup(string backupId)
        {
            string sqlQuery = "SELECT BackupName, EncryptedData AS Content, ContentType, AesKey, AesIv FROM BackupTable WHERE Id = @Id";

            var parameter = new { Id =  backupId };

            var encryptionViewModel = await _dbContext.QueryFirstOrDefaultAsync<EncryptionViewModel>(sqlQuery, parameter);

            var decryptedData = _encryptionService.Decrypt(encryptionViewModel.Content, encryptionViewModel.AesKey, encryptionViewModel.AesIv);

            return new UploadBackupFileViewModel()
            {
                EncryptedData = decryptedData,
                BackupName = encryptionViewModel.BackupName,
                ContentType = encryptionViewModel.ContentType
            };
        }

        public async Task<BackupViewModel> GetBackup(string backupId)
        {
            var backup = await _dbContext.QueryFirstOrDefaultAsync<BackupViewModel>("SELECT Id, BackupName, CreationDate FROM BackupTable WHERE Id = @0 ORDER BY CreationDate DESC", backupId);

            if (backup is null)
                throw new ArgumentNullException(nameof(backup));

            return backup;
        }

        public async Task<IEnumerable<BackupViewModel>> GetBackups()
        {
            var isLogin = await _userContext.IsUserLogIn();

            if (!isLogin)
                return new List<BackupViewModel>();

            var user = await _userContext.GetUser();

            var parameter = new
            {
                UserId = user.Id
            };

            return await _dbContext.QueryAsync<BackupViewModel>("SELECT Id, BackupName, CreationDate, Size FROM BackupTable WHERE UserId = @UserId AND IsDeleted = 0", parameter);
        }
    }
}
