using System.Linq.Expressions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Decorators;

internal sealed class PushSubscriptionQueryDecorator : IPushSubscriptionQueryRepository
{
    private readonly IPushSubscriptionQueryRepository decorated;

    public PushSubscriptionQueryDecorator(IPushSubscriptionQueryRepository decorated)
    {
        this.decorated = decorated;
    }

    public Task<IReadOnlyPushSubscriptionEntity[]> GetAllAsync(TenantId tenant, CancellationToken cancelToken) => this.decorated.GetAsync(x => x.TenantId == tenant.Id, tenant, cancelToken);

    public Task<IReadOnlyPushSubscriptionEntity[]> GetAsync(Expression<Func<IReadOnlyPushSubscriptionEntity, bool>> predicate, TenantId tenant, CancellationToken cancelToken)
    {
        Expression<Func<IReadOnlyPushSubscriptionEntity, bool>> tenantCheck = x => x.TenantId == tenant.Id;
        var newPredicateBody = Expression.AndAlso(tenantCheck.Body, Expression.Invoke(predicate, tenantCheck.Parameters.Single()));
        var newPredicate = Expression.Lambda<Func<IReadOnlyPushSubscriptionEntity, bool>>(newPredicateBody, tenantCheck.Parameters);
        return this.decorated.GetAsync(newPredicate, tenant, cancelToken);
    }
}