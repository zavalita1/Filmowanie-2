using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;
using Filmowanie.Abstractions;

namespace Filmowanie.Database.Interfaces;

public interface IVotingSessionQueryRepository
{
    public Task<IReadonlyVotingResult?> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, TenantId tenant, CancellationToken cancellationToken);
    public Task<IEnumerable<IReadonlyVotingResult>> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, TenantId tenant, Expression<Func<IReadonlyVotingResult, object>> sortBy, int take,
        CancellationToken cancellationToken);
    public Task<IEnumerable<T>> Get<T>(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, T>> selector, TenantId tenant,
        CancellationToken cancellationToken) where T : class;
}