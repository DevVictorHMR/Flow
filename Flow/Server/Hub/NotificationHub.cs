using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SubscribeToUserNotifications(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}