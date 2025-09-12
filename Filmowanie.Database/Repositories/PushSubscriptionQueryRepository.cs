using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal sealed class PushSubscriptionQueryRepository : IPushSubscriptionQueryRepository
{
    private readonly PushSubscriptionsContext _ctx;

    public PushSubscriptionQueryRepository(PushSubscriptionsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadOnlyPushSubscriptionEntity[]> GetAsync(TenantId tenant, CancellationToken cancelToken)
    {
        return await _ctx.Subscriptions.AsNoTracking().ToArrayAsync(cancelToken);
    }

    public async Task<IReadOnlyPushSubscriptionEntity[]> GetAsync(Expression<Func<IReadOnlyPushSubscriptionEntity, bool>> predicate, TenantId tenant, CancellationToken cancelToken)
    {
        return await _ctx.Subscriptions.Where(predicate).AsNoTracking().ToArrayAsync(cancelToken);
    }
}