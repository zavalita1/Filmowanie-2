using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;
using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Interfaces;

public interface IPushSubscriptionQueryRepository
{
    Task<IReadOnlyPushSubscriptionEntity[]> GetAsync(TenantId tenant, CancellationToken cancelToken);
    Task<IReadOnlyPushSubscriptionEntity[]> GetAsync(Expression<Func<IReadOnlyPushSubscriptionEntity, bool>> predicate, TenantId tenant, CancellationToken cancelToken);
}