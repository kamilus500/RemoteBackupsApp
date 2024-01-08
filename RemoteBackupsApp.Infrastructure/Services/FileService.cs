using Microsoft.AspNetCore.Http;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class FileService : IFileService
    {
        public string ConvertFileSize(long fileSize)
        {
            const long kilobyte = 1024;
            const long megabyte = kilobyte * 1024;

            string fileSizeStr = string.Empty;

            if (fileSize < kilobyte)
            {
                fileSizeStr = $"{(double)fileSize} B";
            }
            else if (fileSize < megabyte)
            {
                fileSizeStr = $"{(double)(fileSize / 1024)} KB";
            }
            else
            {
                var size = (double)fileSize / (1024 * 1024);
                fileSizeStr = $"{size.ToString("0.##")} MB";
            }

            return fileSizeStr;
        }

        public async Task<byte[]> GetFileContent(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file));

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
