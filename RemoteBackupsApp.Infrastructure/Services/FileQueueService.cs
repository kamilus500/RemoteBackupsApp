using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using System.Threading.Channels;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class FileQueueService : IFileQueueService
    {
        private readonly Channel<FileUploadRequest> _channel;

        public FileQueueService()
        {
            _channel = Channel.CreateUnbounded<FileUploadRequest>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueFileAsync(FileUploadRequest request)
        {
            await _channel.Writer.WriteAsync(request);
        }

        public ChannelReader<FileUploadRequest> GetReader() => _channel.Reader;
    }
}
