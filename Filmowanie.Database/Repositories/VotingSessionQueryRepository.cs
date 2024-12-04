using System.Linq.Expressions;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal class VotingSessionQueryRepository : IVotingSessionQueryRepository
{
    private readonly VotingResultsContext _ctx;

    public VotingSessionQueryRepository(VotingResultsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadonlyVotingResult?> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var currentVotingSession = await _ctx.VotingResults.SingleOrDefaultAsync(predicate, cancellationToken);
        return currentVotingSession;
    }

    public async Task<IEnumerable<IReadonlyVotingResult>> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, object>> sortBy, int take,
        CancellationToken cancellationToken)
    {
        Func<IQueryable<IReadonlyVotingResult>, IOrderedQueryable<IReadonlyVotingResult>> sortFunction = take > 0 ? x => x.OrderBy(sortBy) : x => x.OrderByDescending(sortBy);
        
        var query = _ctx.VotingResults.Where(predicate);
        var currentVotingSession = await sortFunction.Invoke(query).Take(Math.Abs(take)).ToArrayAsync(cancellationToken);
        return currentVotingSession;
    }

    public async Task<IEnumerable<T>> Get<T>(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, T>> selector, CancellationToken cancellationToken)
    {
        var entities = await _ctx.VotingResults.Where(predicate).Select(selector).ToArrayAsync(cancellationToken);
        return entities;
    }
}