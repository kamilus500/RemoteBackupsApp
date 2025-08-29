using Microsoft.AspNetCore.Http;

namespace RemoteBackupsApp.Infrastructure.Helpers
{
    public static class FileHelper
    {
        public static async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Array.Empty<byte>();

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
