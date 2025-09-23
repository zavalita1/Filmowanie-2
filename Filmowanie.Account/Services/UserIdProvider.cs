using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Filmowanie.Account.Services;

public sealed class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.Claims.SingleOrDefault(x => x.Type == ClaimsTypes.UserName)?.Value;
    }
}