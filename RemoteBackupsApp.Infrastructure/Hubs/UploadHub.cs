using Microsoft.AspNetCore.SignalR;

namespace RemoteBackupsApp.Infrastructure.Hubs
{
    public class UploadHub : Hub
    {
        public async Task JoinJob(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }
    }
}
