using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace Filmowanie.Handlers;

public sealed class UserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User.Claims.SingleOrDefault(x => x.Type == "username")?.Value;
    }
}