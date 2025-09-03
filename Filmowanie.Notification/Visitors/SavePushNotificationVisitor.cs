using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Repositories;
using Filmowanie.Notification.DTOs.Incoming;
using Filmowanie.Notification.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Visitors;

internal sealed class SavePushNotificationVisitor : ISavePushNotificationVisitor
{
    private readonly IPushSubscriptionCommandRepository _iPushSubscriptionCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SavePushNotificationVisitor(IPushSubscriptionCommandRepository iPushSubscriptionCommandRepository, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider, ILogger<SavePushNotificationVisitor> log)
    {
        _iPushSubscriptionCommandRepository = iPushSubscriptionCommandRepository;
        _guidProvider = guidProvider;
        _dateTimeProvider = dateTimeProvider;
        Log = log;
    }

    public async Task<OperationResult<object>> SignUp(OperationResult<(PushSubscriptionDTO, DomainUser)> input, CancellationToken cancellationToken)
    {
        var id = _guidProvider.NewGuid();
        var user = input.Result.Item2;
        var tenant = user.Tenant;
        var embeddedUser = new EmbeddedUser { id = user.Id, Name = user.Name, TenantId = tenant.Id };
        var entity = new IReadOnlyPushNotificationData(id.ToString(), _dateTimeProvider.Now, tenant.Id, input.Result.Item1.P256DH, input.Result.Item1.Auth, input.Result.Item1.Endpoint, embeddedUser);

        await _iPushSubscriptionCommandRepository.InsertAsync(entity, cancellationToken);
        return OperationResultExtensions.Empty;
    }

    public ILogger Log { get; }

    private record struct IReadOnlyPushNotificationData(string id, DateTime Created, int TenantId, string P256DH, string Auth, string Endpoint, IReadOnlyEmbeddedUser User) : IReadOnlyPushSubscriptionEntity;
}
