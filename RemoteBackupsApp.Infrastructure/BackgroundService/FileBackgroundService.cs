using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Domain.Models;
using System;

public class FileBackgroundService : BackgroundService
{
    private readonly IFileQueueService _fileQueue;
    private readonly IServiceProvider _services;
    private readonly IWebHostEnvironment _env;

    public FileBackgroundService(IFileQueueService fileQueue, IServiceProvider services, IWebHostEnvironment env)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _fileQueue = fileQueue ?? throw new ArgumentNullException(nameof(fileQueue));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _fileQueue.GetReader();

        await foreach (var fileRequest in reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads") + $"\\{fileRequest.UserId}";

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileRequest.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.WriteAsync(fileRequest.File, 0, fileRequest.File.Length);
                }

                Console.WriteLine($"Plik zapisany: {filePath}");

                using var scope = _services.CreateScope();
                var filesRepository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();

                await filesRepository.SaveFile(new FileDto()
                {
                    FileSize = fileRequest.FileSize,
                    CreatedAt = fileRequest.CreatedAt,
                    FileExtension = Path.GetExtension(fileRequest.FileName),
                    FileName = fileRequest.FileName,
                    FilePath = filePath,
                    UserId = fileRequest.UserId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu pliku: {ex.Message}");
            }
        }
    }
}
