namespace RemoteBackupsApp.Domain.Models
{
    public record UpdateProgress(int processId, int percent, string status, DateTime? date, string? fileName = null);
}
