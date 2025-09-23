using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Enums;
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

// TODO UTs
internal sealed class PushNotificationService : IPushNotificationService
{
    private readonly IPushSubscriptionCommandRepository pushSubscriptionCommandRepository;
    private readonly IPushSubscriptionQueryRepository pushSubscriptionQueryRepository;
    private readonly PushNotificationOptions options;
    private readonly IGuidProvider guidProvider;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ILogger<PushNotificationService> log;

    public PushNotificationService(IPushSubscriptionCommandRepository pushSubscriptionCommandRepository, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider, ILogger<PushNotificationService> log, IPushSubscriptionQueryRepository pushSubscriptionQueryRepository, IOptions<PushNotificationOptions> options)
    {
        this.pushSubscriptionCommandRepository = pushSubscriptionCommandRepository;
        this.guidProvider = guidProvider;
        this.dateTimeProvider = dateTimeProvider;
        this.log = log;
        this.pushSubscriptionQueryRepository = pushSubscriptionQueryRepository;
        this.options = options.Value;
    }

    public Task<Maybe<VoidResult>> SavePushNotification(Maybe<PushSubscriptionDTO> maybeDto, Maybe<DomainUser> maybeUser, CancellationToken cancelToken) =>
        maybeDto.Merge(maybeUser).AcceptAsync(SavePushNotification, this.log, cancelToken);

    public Task<Maybe<VoidResult>> SendAllPushNotificationsAsync(Maybe<TenantId> maybeTenant, Maybe<string> maybeMessage, CancellationToken cancelToken) => maybeTenant.Merge(maybeMessage).AcceptAsync(SendAllPushNotificationsAsync, this.log, cancelToken);

    private async Task<Maybe<VoidResult>> SavePushNotification((PushSubscriptionDTO, DomainUser) input, CancellationToken cancelToken)
    {
        if (!this.options.Enabled)
            return VoidResult.Void;

        var id = this.guidProvider.NewGuid();
        var user = input.Item2;
        var tenant = user.Tenant;
        var embeddedUser = new EmbeddedUser { id = user.Id, Name = user.Name, TenantId = tenant.Id };
        var entity = new IReadOnlyPushNotificationData(id.ToString(), this.dateTimeProvider.Now, tenant.Id, input.Item1.p256dh, input.Item1.Auth, input.Item1.Endpoint, embeddedUser);

        await this.pushSubscriptionCommandRepository.InsertAsync(entity, cancelToken);
        return VoidResult.Void;
    }

    public async Task<Maybe<VoidResult>> SendAllPushNotificationsAsync((TenantId, string Message) input, CancellationToken cancelToken)
    {
        var pushSubscriptions = await this.pushSubscriptionQueryRepository.GetAllAsync(input.Item1, cancelToken);
        var errors = new ConcurrentStack<Maybe<VoidResult>>();
        var corruptSubscriptions = new ConcurrentBag<IReadOnlyPushSubscriptionEntity>();

        await Parallel.ForEachAsync(pushSubscriptions, cancelToken, async (pushNotificationSubscription, ct) =>
        {
            {
                var vapidDetails = new VapidDetails(this.options.Subject, this.options.PublicKey, this.options.PrivateKey);
                using var client = new WebPushClient();
                var pushNotification = new PushSubscription(pushNotificationSubscription.Endpoint, pushNotificationSubscription.P256DH, pushNotificationSubscription.Auth);

                try
                {
                    await client.SendNotificationAsync(pushNotification, input.Item2, vapidDetails, ct);
                }
                catch (WebPushException ex)
                {
                    var message = "Error when trying to push notification..";
                    this.log.LogError(ex, message);
                    errors.Push(new Error<VoidResult>(message, ErrorType.Network));
                    corruptSubscriptions.Add(pushNotificationSubscription);
                }
                catch (Exception ex)
                {
                    var message = $"Error when trying to push notification to: {pushNotificationSubscription.User.Name}.";
                    this.log.LogError(ex, message);
                    errors.Push(new Error<VoidResult>(message, ErrorType.Network));
                }
            }
        });

        await this.pushSubscriptionCommandRepository.DeleteAsync(corruptSubscriptions, cancelToken);

        var result = errors.Aggregate(VoidResult.Void, (curr, agg) => agg.Merge(curr));
        return result;
    }


    private record struct IReadOnlyPushNotificationData(string id, DateTime Created, int TenantId, string P256DH, string Auth, string Endpoint, IReadOnlyEmbeddedUser User) : IReadOnlyPushSubscriptionEntity;
}
