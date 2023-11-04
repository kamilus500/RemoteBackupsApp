
using Microsoft.AspNetCore.Http;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IFileService
    {
        Task<byte[]> GetFileContent(IFormFile file);
    }
}
