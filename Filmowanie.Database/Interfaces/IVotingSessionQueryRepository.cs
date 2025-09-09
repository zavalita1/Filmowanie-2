using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;

namespace Filmowanie.Database.Interfaces;

internal interface IVotingSessionQueryRepository
{
    Task<IReadOnlyVotingResult?> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, CancellationToken cancelToken);
    Task<IEnumerable<IReadOnlyVotingResult>> GetAll(Expression<Func<IReadOnlyVotingResult, bool>> query, CancellationToken cancelToken);
    Task<IEnumerable<IReadOnlyVotingResult>> GetVotingResultAsync(Expression<Func<IReadOnlyVotingResult, bool>> predicate, Expression<Func<IReadOnlyVotingResult, object>> sortBy, int take,
        CancellationToken cancelToken);
}