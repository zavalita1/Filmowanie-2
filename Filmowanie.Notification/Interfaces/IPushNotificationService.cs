using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Notification.DTOs.Incoming;

namespace Filmowanie.Notification.Interfaces;

internal interface IPushNotificationService
{
    Task<Maybe<VoidResult>> SavePushNotification(Maybe<(PushSubscriptionDTO, DomainUser)> input, CancellationToken cancelToken);
    
    Task<Maybe<object>> SendAllPushNotificationsAsync(Maybe<(TenantId, string Message)> input, CancellationToken cancelToken);
}