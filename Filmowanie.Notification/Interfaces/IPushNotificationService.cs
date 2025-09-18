using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Notification.DTOs.Incoming;

namespace Filmowanie.Notification.Interfaces;

internal interface IPushNotificationService
{
    Task<Maybe<VoidResult>> SavePushNotification(Maybe<PushSubscriptionDTO> maybeDto, Maybe<DomainUser> maybeUser, CancellationToken cancelToken);
    
    Task<Maybe<VoidResult>> SendAllPushNotificationsAsync(Maybe<TenantId> maybeTenant, Maybe<string> maybeMessage, CancellationToken cancelToken);
}