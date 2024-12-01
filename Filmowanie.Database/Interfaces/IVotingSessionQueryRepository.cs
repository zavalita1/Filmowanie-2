using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;

namespace Filmowanie.Database.Interfaces;

public interface IVotingSessionQueryRepository
{
    public Task<IReadonlyVotingResult?> GetCurrent(TenantId tenantId, CancellationToken cancellationToken);
    public Task<IEnumerable<IReadonlyVotingResult>> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, object>> sortBy, int take,
        CancellationToken cancellationToken);
}