using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Notification.DTOs.Incoming;

namespace Filmowanie.Notification.Interfaces;

internal interface IPushNotificationService
{
    Task<OperationResult<VoidResult>> SavePushNotification(OperationResult<(PushSubscriptionDTO, DomainUser)> input, CancellationToken cancelToken);
    
    Task<OperationResult<object>> SendAllPushNotificationsAsync(OperationResult<(TenantId, string Message)> input, CancellationToken cancelToken);
}