using Microsoft.AspNetCore.SignalR;

namespace Filmowanie.Notification;

public sealed class VotingStateHub : Hub
{
    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        return base.OnConnectedAsync();
    }
}