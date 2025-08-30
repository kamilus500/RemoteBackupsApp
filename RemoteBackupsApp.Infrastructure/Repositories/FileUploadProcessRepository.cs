using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Repositories
{
    public class FileUploadProcessRepository : IFileUploadProcessRepository
    {
        private readonly ISqlService _sqlService;
        public FileUploadProcessRepository(ISqlService sqlService)
        {
            _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
        }

        public async Task<int> Create(int userId, int fileId)
        {
            var parameters = new
            {
                UserId = userId,
                FileId = fileId
            };

            return await _sqlService.ExecuteAsync("dbo.CreateFileUploadRequest", parameters, CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<FileUploadProgress>> GetByUserId(int userId)
            => await _sqlService.QueryAsync<FileUploadProgress>(
                    "SELECT * FROM dbo.vwFileUploadProgress WHERE UserId = @UserId Order By CreatedAt desc",
                    new { UserId = userId },
                    CommandType.Text
                );

        public async Task UpdateProgress(int progressId, decimal progressPct, string status)
        {
            var parameters = new
            {
                ProgressId = progressId,
                ProgressPct = progressPct,
                Status = status
            };

            await _sqlService.ExecuteAsync("dbo.UpdateFileUploadRequest", parameters, CommandType.StoredProcedure);
        }
    }
}
