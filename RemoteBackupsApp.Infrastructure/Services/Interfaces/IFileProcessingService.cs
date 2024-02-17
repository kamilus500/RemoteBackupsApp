using RemoteBackupsApp.Domain.ViewModels.Backup;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IFileProcessingService
    {
        void ProcessFile(FileProcessViewModel fileProcessViewModel);
    }
}
