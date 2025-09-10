using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IPushSubscriptionCommandRepository
{
    Task InsertAsync(IReadOnlyPushSubscriptionEntity entity, CancellationToken cancelToken);
    Task DeleteAsync(IEnumerable<IReadOnlyPushSubscriptionEntity> entities, CancellationToken cancelToken);
}