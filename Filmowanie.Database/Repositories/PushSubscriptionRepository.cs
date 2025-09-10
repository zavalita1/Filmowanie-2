using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Repositories;

internal sealed class PushSubscriptionCommandRepository : IPushSubscriptionCommandRepository
{
    private readonly PushSubscriptionsContext _ctx;

    public PushSubscriptionCommandRepository(PushSubscriptionsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task InsertAsync(IReadOnlyPushSubscriptionEntity entity, CancellationToken cancelToken)
    {
        var dbEntity = new ReadOnlyPushSubscriptionEntity(entity);
        await _ctx.Subscriptions.AddAsync(dbEntity, cancelToken);
        await _ctx.SaveChangesAsync(cancelToken);
    }

    public async Task DeleteAsync(IEnumerable<IReadOnlyPushSubscriptionEntity> entities, CancellationToken cancelToken)
    {
        var dbEntities = entities.Select(x => new ReadOnlyPushSubscriptionEntity(x));
        _ctx.Subscriptions.RemoveRange(dbEntities);
        await _ctx.SaveChangesAsync(cancelToken);
    }
}