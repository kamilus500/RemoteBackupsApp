using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using RemoteBackupsApp.Infrastructure.Hubs;

public class FileBackgroundService : BackgroundService
{
    private readonly IFileQueueService _fileQueue;
    private readonly IServiceProvider _services;
    private readonly IHubContext<UploadHub> _hub;
    private readonly IWebHostEnvironment _env;

    private const int BufferSize = 6 * 1024;

    public FileBackgroundService(
        IFileQueueService fileQueue,
        IHubContext<UploadHub> hub,
        IServiceProvider services,
        IWebHostEnvironment env)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _fileQueue = fileQueue ?? throw new ArgumentNullException(nameof(fileQueue));
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _fileQueue.GetReader();

        await foreach (var fileRequest in reader.ReadAllAsync(stoppingToken))
        {
            await ProcessFileAsync(fileRequest, stoppingToken);
        }
    }

    private async Task ProcessFileAsync(FileUploadRequest fileRequest, CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var filesUploadRepository = scope.ServiceProvider.GetRequiredService<IFileUploadProcessRepository>();

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", fileRequest.UserId.ToString());

        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, fileRequest.FileName);

        int percent = 0;

        try
        {
            long totalBytes = fileRequest.File.Length;
            long writtenBytes = 0;

            using var stream = new MemoryStream(fileRequest.File);
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[BufferSize];
            int read;

            while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), stoppingToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), stoppingToken);

                writtenBytes += read;
                percent = (int)(writtenBytes * 100.0 / totalBytes);

                string status = percent < 100 ? "Uploading" : "Completed";

                await UpdateProgressAsync(filesUploadRepository, fileRequest.ProcessId, percent, status, stoppingToken);
            }

            await UploadSuccessOrFailedAsync(fileRequest.FileName, true, stoppingToken);
        }
        catch (Exception ex)
        {
            await UpdateProgressAsync(filesUploadRepository, fileRequest.ProcessId, percent, "Failed", stoppingToken, fileName: fileRequest.FileName);
            await UploadSuccessOrFailedAsync(fileRequest.FileName, false, stoppingToken);
        }
    }

    private async Task UpdateProgressAsync(
        IFileUploadProcessRepository repository,
        int processId,
        int percent,
        string status,
        CancellationToken cancellationToken,
        bool completed = false,
        string? fileName = null)
    {
        await repository.UpdateProgress(processId, percent, status);

        var updateProgress = new UpdateProgress(processId, percent, status, completed ? DateTime.UtcNow : null, fileName);

        await _hub.Clients.All.SendAsync("ProgressUpdated", updateProgress, cancellationToken: cancellationToken);
    }
    
    private async Task UploadSuccessOrFailedAsync(
        string fileName,
        bool isCompleted,
        CancellationToken cancellationToken)
    {
        var result = new
        {
            FileName = fileName,
            IsCompleted = isCompleted
        };

        await _hub.Clients.All.SendAsync("UploadSuccess", result, cancellationToken: cancellationToken);
    }
}
