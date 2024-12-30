using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;
using Filmowanie.Abstractions;

namespace Filmowanie.Database.Interfaces;

public interface IVotingSessionQueryRepository
{
    public Task<IReadOnlyVotingResult?> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, TenantId tenant, CancellationToken cancellationToken);
    public Task<IEnumerable<IReadOnlyVotingResult>> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, TenantId tenant, Expression<Func<IReadOnlyVotingResult, object>> sortBy, int take,
        CancellationToken cancellationToken);
    public Task<IEnumerable<T>> Get<T>(Expression<Func<IReadOnlyVotingResult, bool>> predicate, Expression<Func<IReadOnlyVotingResult, T>> selector, TenantId tenant,
        CancellationToken cancellationToken) where T : class;
}