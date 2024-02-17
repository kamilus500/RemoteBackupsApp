using Microsoft.AspNetCore.SignalR;

namespace RemoteBackupsApp.Infrastructure.Hubs
{
    public class NotyfyCompleteTaskHub : Hub
    {
        public async Task NotifyJobCompleted()
        {
            await Clients.All.SendAsync("JobCompleted");
        }
    }
}
