using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Notification.DTOs.Incoming;
using Filmowanie.Notification.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Microsoft.Extensions.Options;
using WebPush;

namespace Filmowanie.Notification.Services;

internal sealed class PushNotificationService : IPushNotificationService
{
    private readonly IPushSubscriptionCommandRepository _pushSubscriptionCommandRepository;
    private readonly IPushSubscriptionQueryRepository _pushSubscriptionQueryRepository;
    private readonly PushNotificationOptions _options;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<PushNotificationService> _log;

    public PushNotificationService(IPushSubscriptionCommandRepository pushSubscriptionCommandRepository, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider, ILogger<PushNotificationService> log, IPushSubscriptionQueryRepository pushSubscriptionQueryRepository, IOptions<PushNotificationOptions> options)
    {
        _pushSubscriptionCommandRepository = pushSubscriptionCommandRepository;
        _guidProvider = guidProvider;
        _dateTimeProvider = dateTimeProvider;
        _log = log;
        _pushSubscriptionQueryRepository = pushSubscriptionQueryRepository;
        _options = options.Value;
    }

    public Task<Maybe<VoidResult>> SavePushNotification(Maybe<(PushSubscriptionDTO, DomainUser)> input, CancellationToken cancelToken) =>
        input.AcceptAsync(SavePushNotification, _log, cancelToken);

    public Task<Maybe<VoidResult>> SendAllPushNotificationsAsync(Maybe<(TenantId, string Message)> input, CancellationToken cancelToken) => input.AcceptAsync(SendAllPushNotificationsAsync, _log, cancelToken);

    private async Task<Maybe<VoidResult>> SavePushNotification((PushSubscriptionDTO, DomainUser) input, CancellationToken cancelToken)
    {
        var id = _guidProvider.NewGuid();
        var user = input.Item2;
        var tenant = user.Tenant;
        var embeddedUser = new EmbeddedUser { id = user.Id, Name = user.Name, TenantId = tenant.Id };
        var entity = new IReadOnlyPushNotificationData(id.ToString(), _dateTimeProvider.Now, tenant.Id, input.Item1.p256dh, input.Item1.Auth, input.Item1.Endpoint, embeddedUser);

        await _pushSubscriptionCommandRepository.InsertAsync(entity, cancelToken);
        return VoidResult.Void;
    }

    public async Task<Maybe<VoidResult>> SendAllPushNotificationsAsync((TenantId, string Message) input, CancellationToken cancelToken)
    {
        var pushSubscriptions = await _pushSubscriptionQueryRepository.GetAsync(input.Item1, cancelToken);
        var errors = new ConcurrentStack<Maybe<VoidResult>>();
        var corruptSubscriptions = new ConcurrentBag<IReadOnlyPushSubscriptionEntity>();

        await Parallel.ForEachAsync(pushSubscriptions, cancelToken, async (pushNotificationSubscription, ct) =>
        {
            {
                var vapidDetails = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
                using var client = new WebPushClient();
                var pushNotification = new PushSubscription(pushNotificationSubscription.Endpoint, pushNotificationSubscription.P256DH, pushNotificationSubscription.Auth);

                try
                {
                    await client.SendNotificationAsync(pushNotification, input.Item2, vapidDetails, ct);
                }
                catch (WebPushException ex)
                {
                    var message = "Error when trying to push notification..";
                    _log.LogError(ex, message);
                    errors.Push(new Error<VoidResult>(message, ErrorType.Network));
                    corruptSubscriptions.Add(pushNotificationSubscription);
                }
                catch (Exception ex)
                {
                    var message = $"Error when trying to push notification to: {pushNotificationSubscription.User.Name}.";
                    _log.LogError(ex, message);
                    errors.Push(new Error<VoidResult>(message, ErrorType.Network));
                }
            }
        });

        await _pushSubscriptionCommandRepository.DeleteAsync(corruptSubscriptions, cancelToken);

        var result = errors.Aggregate(VoidResult.Void, (curr, agg) => agg.Merge(curr));
        return result;
    }


    private record struct IReadOnlyPushNotificationData(string id, DateTime Created, int TenantId, string P256DH, string Auth, string Endpoint, IReadOnlyEmbeddedUser User) : IReadOnlyPushSubscriptionEntity;
}
