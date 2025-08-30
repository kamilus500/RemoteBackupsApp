namespace RemoteBackupsApp.Domain.Models
{
    public class FileUploadProgress
    {
        public int ProgressId { get; set; }

        public string FileName { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }

        public decimal ProgressPct { get; set; } = 0m;

        public string Status { get; set; } = "Pending"; // Pending, Uploading, Completed, Failed

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
