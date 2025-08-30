using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Repositories
{
    public class FilesRepository : IFilesRepository
    {
        private readonly ISqlService _sqlService;
        public FilesRepository(ISqlService sqlService)
        {
            _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
        }

        public async Task<int> Delete(int fileId, int userId)
        {
            var parameters = new
            {
                UserId = userId,
                FileId = fileId
            };

            return await _sqlService.QuerySingleAsync<int>("dbo.DeleteFile", parameters, CommandType.StoredProcedure);
        }

        public async Task<string> GetFilePath(int fileId)
            => await _sqlService.QuerySingleAsync<string?>(
                            "SELECT FilePath FROM dbo.Files WHERE FileId = @FileId",
                            new { FileId = fileId },
                            CommandType.Text);
            

        public async Task<IEnumerable<FileDto>> GetFiles(int userId)

            => await _sqlService.QueryAsync<FileDto>(
                    "SELECT * FROM dbo.vwUserFiles WHERE UserId = @UserId And IsDeleted = 0",
                    new { UserId = userId },
                    CommandType.Text
                );

        public async Task SaveFile(FileDto file)
        {
            var parameters = new
            {
                UserId = file.UserId,
                FileName = file.FileName,
                FileExtension = file.FileExtension,
                FileSize = file.FileSize,
                FilePath = file.FilePath,
                CreatedAt = file.CreatedAt
            };

            await _sqlService.ExecuteAsync("dbo.InsertFile", parameters, CommandType.StoredProcedure);
        }
    }
}
