using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Repositories;

internal sealed class PushSubscriptionCommandRepository : IPushSubscriptionCommandRepository
{
    private readonly PushSubscriptionsContext ctx;

    public PushSubscriptionCommandRepository(PushSubscriptionsContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task InsertAsync(IReadOnlyPushSubscriptionEntity entity, CancellationToken cancelToken)
    {
        var dbEntity = new ReadOnlyPushSubscriptionEntity(entity);
        await this.ctx.Subscriptions.AddAsync(dbEntity, cancelToken);
        await this.ctx.SaveChangesAsync(cancelToken);
    }

    public async Task DeleteAsync(IEnumerable<IReadOnlyPushSubscriptionEntity> entities, CancellationToken cancelToken)
    {
        var dbEntities = entities.Select(x => new ReadOnlyPushSubscriptionEntity(x));
        this.ctx.Subscriptions.RemoveRange(dbEntities);
        await this.ctx.SaveChangesAsync(cancelToken);
    }
}