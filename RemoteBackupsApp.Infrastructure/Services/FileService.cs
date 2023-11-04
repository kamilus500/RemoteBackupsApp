using Microsoft.AspNetCore.Http;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class FileService : IFileService
    {
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
