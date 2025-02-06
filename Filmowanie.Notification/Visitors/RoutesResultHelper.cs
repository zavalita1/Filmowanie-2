using System.Collections.Concurrent;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces;
using Filmowanie.Notification.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace Filmowanie.Notification.Visitors;

internal class NotifyAllPushSubscribersVisitor : INotifyAllPushSubscribersVisitor
{
    private readonly IPushSubscriptionQueryRepository _pushSubscriptionQueryRepository;
    private readonly PushNotificationOptions _options;

    public NotifyAllPushSubscribersVisitor(IPushSubscriptionQueryRepository pushSubscriptionQueryRepository, IOptions<PushNotificationOptions> options, ILogger<NotifyAllPushSubscribersVisitor> log)
    {
        _pushSubscriptionQueryRepository = pushSubscriptionQueryRepository;
        _options = options.Value;
        Log = log;
    }

    public async Task<OperationResult<object>> VisitAsync(OperationResult<(TenantId, string Message)> input, CancellationToken cancellationToken)
    {
        var pushSubscriptions = await _pushSubscriptionQueryRepository.GetAsync(input.Result.Item1, cancellationToken);
        var errors = new ConcurrentStack<Error>();

        await Parallel.ForEachAsync(pushSubscriptions, cancellationToken, async (pushNotificationSubscription, ct) =>
        {
            {
                var vapidDetails = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
                using var client = new WebPushClient();
                var pushNotification = new PushSubscription(pushNotificationSubscription.Endpoint, pushNotificationSubscription.P256DH, pushNotificationSubscription.Auth);

                try
                {
                    await client.SendNotificationAsync(pushNotification, input.Result.Item2, vapidDetails, ct);
                }
                catch (Exception ex)
                {
                    var message = $"Error when trying to push notification to: {pushNotificationSubscription.User.Name}.";
                    Log.LogError(ex, message);
                    errors.Push(new Error(message, ErrorType.Network));
                }
            }
        });

        return new OperationResult<object>(errors);
    }

    public ILogger Log { get; }
}