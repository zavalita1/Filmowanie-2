using Filmowanie.Notification.DTOs.Incoming;
using Filmowanie.Notification.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Notification.Routes;

internal sealed class PushNotificationRoutes : IPushNotificationRoutes
{
    public async Task<IResult> Add(PushSubscriptionDTO dto, CancellationToken cancellationToken)
    {
        return null;
    }
}