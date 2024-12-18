using Filmowanie.Notification.DTOs.Incoming;
using Filmowanie.Notification.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Filmowanie.Notification.Extensions;

public static class RouteGroupBuilderExtensions
{
    public const string VotesHubPath = "/votesHub";

    public static RouteGroupBuilder RegisterNotificationRoutes(this RouteGroupBuilder builder)
    {
        var accountRoutesBuilder = builder.MapGroup("pushNotification/").RequireAuthorization();

        accountRoutesBuilder.MapPost("add", ([FromServices] IPushNotificationRoutes routes, [FromBody] PushSubscriptionDTO dto, CancellationToken ct) => routes.Add(dto, ct));

        builder.MapHub<VotingStateHub>(VotesHubPath);

        return builder;
    }
}