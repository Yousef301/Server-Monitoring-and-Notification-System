using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Hubs;

public class AlertHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}