using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Infrastructure.Hubs;

public class FileBackgroundService : BackgroundService
{
    private readonly IFileQueueService _fileQueue;
    private readonly IServiceProvider _services;
    private readonly IHubContext<UploadHub> _hub;
    private readonly IWebHostEnvironment _env;

    public FileBackgroundService(IFileQueueService fileQueue, IHubContext<UploadHub> hub, IServiceProvider services, IWebHostEnvironment env)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _fileQueue = fileQueue ?? throw new ArgumentNullException(nameof(fileQueue));
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _fileQueue.GetReader();

        await foreach (var fileRequest in reader.ReadAllAsync(stoppingToken))
        {
            int percent = 0;
            using var scope = _services.CreateScope();
            var filesUploadRepository = scope.ServiceProvider.GetRequiredService<IFileUploadProcessRepository>();

            try
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads") + $"\\{fileRequest.UserId}";

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileRequest.FileName);

                long totalBytes = fileRequest.File.Length;
                long writtenBytes = 0;
                int bufferSize = 1024*6;
                byte[] buffer = new byte[bufferSize];

                using (var stream = new MemoryStream(fileRequest.File))
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read, stoppingToken);

                        writtenBytes += read;
                        percent = (int)((writtenBytes * 100.0) / totalBytes);
                        Thread.Sleep(20);

                        await filesUploadRepository.UpdateProgress(fileRequest.ProcessId, percent, "Uploading");

                        await _hub.Clients.All
                            .SendAsync("ProgressUpdated", new
                            {
                                processId = fileRequest.ProcessId,
                                percentage = percent,
                                status = "Uploading"
                            }, cancellationToken: stoppingToken);
                    }
                }

                await filesUploadRepository.UpdateProgress(fileRequest.ProcessId, 100, "Completed");

                await _hub.Clients.All
                    .SendAsync("ProgressUpdated", new
                    {
                        processId = fileRequest.ProcessId,
                        percentage = 100,
                        status = "Completed",
                        date = DateTime.UtcNow
                    }, cancellationToken: stoppingToken);

                Console.WriteLine($"Plik zapisany: {filePath}");
            }
            catch (Exception ex)
            {
                await filesUploadRepository.UpdateProgress(fileRequest.ProcessId, percent, "Failed");
                await _hub.Clients.All
                    .SendAsync("ProgressUpdated", new
                    {
                        processId = fileRequest.ProcessId,
                        percentage = percent,
                        status = "Failed"
                    }, cancellationToken: stoppingToken);
                Console.WriteLine($"Błąd podczas zapisu pliku: {ex.Message}");
            }
        }
    }
}
