using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Notification.DTOs.Incoming;

namespace Filmowanie.Notification.Interfaces;

internal interface IPushNotificationService
{
    Task<Maybe<VoidResult>> SavePushNotification(Maybe<(PushSubscriptionDTO, DomainUser)> input, CancellationToken cancelToken);
    
    Task<Maybe<VoidResult>> SendAllPushNotificationsAsync(Maybe<(TenantId, string Message)> input, CancellationToken cancelToken);
}