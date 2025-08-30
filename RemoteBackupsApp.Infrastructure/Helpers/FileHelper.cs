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

        public static string FormatBytes(long bytes)
        {
            const double KB = 1024;
            const double MB = KB * 1024;

            if (bytes >= MB)
            {
                return $"{bytes / MB:F2} MB";
            }
            else if (bytes >= KB)
            {
                return $"{bytes / KB:F2} KB";
            }
            else
            {
                return $"{bytes} B";
            }
        }
    }
}
