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

        public async Task<IEnumerable<FileUploadProgress>> GetByUserId(
            int userId, int pageNumber, int pageSize, string sortColumn = "", string sortDirection = "desc")
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var allowedColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "FileName", "FileName" },
                    { "ProgressPct", "ProgressPct" },
                    { "Status", "Status" },
                    { "StartedAt", "StartedAt" },
                    { "UpdatedAt", "UpdatedAt" },
                    { "CompletedAt", "CompletedAt" },
                    { "CreatedAt", "CreatedAt" }
                };

            if (!allowedColumns.ContainsKey(sortColumn))
                sortColumn = "CreatedAt";

            if (!string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
                sortDirection = "desc";

            var sql = $@"
                        SELECT *
                        FROM dbo.vwFileUploadProgress
                        WHERE UserId = @UserId
                        ORDER BY {sortColumn} {sortDirection}
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                    ";

            return await _sqlService.QueryAsync<FileUploadProgress>(
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

        public async Task<int> GetFilesUploadsCount(int userId)
        {
            var sql = @"SELECT COUNT(*) 
                FROM dbo.vwFileUploadProgress 
                WHERE UserId = @UserId";

            return await _sqlService.QuerySingleAsync<int>(
                sql,
                new { UserId = userId },
                CommandType.Text
            );
        }
    }
}
