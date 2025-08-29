using RemoteBackupsApp.Domain.Models;
using System.Threading.Channels;

namespace RemoteBackupsApp.Domain.Interfaces
{
    public interface IFileQueueService
    {
        ValueTask EnqueueFileAsync(FileUploadRequest request);
        ChannelReader<FileUploadRequest> GetReader();
    }
}
