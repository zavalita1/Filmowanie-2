using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;

namespace Filmowanie.Database.Interfaces;

public interface IVotingSessionQueryRepository
{
    public Task<IReadonlyVotingResult?> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, CancellationToken cancellationToken);
    public Task<IEnumerable<IReadonlyVotingResult>> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, object>> sortBy, int take, CancellationToken cancellationToken);
    public Task<IEnumerable<T>> Get<T>(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, T>> selector, CancellationToken cancellationToken) where T : class;
}