using Filmowanie.Notification.DTOs.Incoming;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Notification.Interfaces;

internal interface IPushNotificationRoutes
{
    Task<IResult> Add(PushSubscriptionDTO dto, CancellationToken cancellationToken);
    Task<IResult> Notify(NotifyDTO dto, CancellationToken cancellationToken);

    Task<IResult> GetVapidPublicKey(CancellationToken cancellationToken);
}