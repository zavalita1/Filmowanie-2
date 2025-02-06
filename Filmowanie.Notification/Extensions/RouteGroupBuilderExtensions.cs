using Filmowanie.Abstractions.Constants;
using Filmowanie.Notification.DTOs.Incoming;
using Filmowanie.Notification.Interfaces;
using Filmowanie.Notification.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Filmowanie.Notification.Extensions;

public static class RouteGroupBuilderExtensions
{
    public const string VotesHubPath = "/votesHub";

    public static RouteGroupBuilder RegisterNotificationRoutes(this RouteGroupBuilder builder)
    {
        var accountRoutesBuilder = builder.MapGroup("pushNotification/");

        accountRoutesBuilder.MapGet("vapid", ([FromServices] IPushNotificationRoutes routes, CancellationToken ct) => routes.GetVapidPublicKey(ct));
        accountRoutesBuilder.MapPost("add", ([FromServices] IPushNotificationRoutes routes, [FromBody] PushSubscriptionDTO dto, CancellationToken ct) => routes.Add(dto, ct)).RequireAuthorization();
        accountRoutesBuilder.MapPost("notify", ([FromServices] IPushNotificationRoutes routes, [FromBody] NotifyDTO dto, CancellationToken ct) => routes.Notify(dto, ct)).RequireAuthorization(Schemes.Admin);

        builder.MapHub<VotingStateHub>(VotesHubPath);

        return builder;
    }
}