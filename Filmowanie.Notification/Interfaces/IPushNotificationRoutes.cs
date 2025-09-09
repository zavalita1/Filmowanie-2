using Filmowanie.Notification.DTOs.Incoming;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Notification.Interfaces;

internal interface IPushNotificationRoutes
{
    Task<IResult> Add(PushSubscriptionDTO dto, CancellationToken cancelToken);
    Task<IResult> Notify(NotifyDTO dto, CancellationToken cancelToken);

    Task<IResult> GetVapidPublicKey(CancellationToken cancelToken);
}