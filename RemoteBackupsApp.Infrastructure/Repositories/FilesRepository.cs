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
            

        public async Task<IEnumerable<FileDto>> GetFiles(int userId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var sql = @"
                        SELECT *
                        FROM dbo.vwUserFiles
                        WHERE UserId = @UserId AND IsDeleted = 0
                        ORDER BY CreatedAt, FileId DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                    ";

            return await _sqlService.QueryAsync<FileDto>(
                sql,
                new
                {
                    UserId = userId,
                    Offset = (pageNumber - 1) * pageSize,
                    PageSize = pageSize
                },
                CommandType.Text
            );
        }

        public async Task<int> SaveFile(FileDto file)
        {
            var parameters = new
            {
                UserId = file.UserId,
                FileName = file.FileName,
                FileExtension = file.FileExtension,
                FileSize = file.FileSize,
                FilePath = file.FilePath
            };

            return await _sqlService.ExecuteAsync("dbo.InsertFile", parameters, CommandType.StoredProcedure);
        }

        public async Task<int> GetFilesCount(int userId)
        {
            var sql = @"SELECT COUNT(*) 
                FROM dbo.vwUserFiles 
                WHERE UserId = @UserId AND IsDeleted = 0";

            return await _sqlService.QuerySingleAsync<int>(
                sql,
                new { UserId = userId },
                CommandType.Text
            );
        }
    }
}
