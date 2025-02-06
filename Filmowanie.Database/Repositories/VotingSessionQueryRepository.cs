using System.Linq.Expressions;
using Filmowanie.Abstractions;
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

    public async Task<IReadOnlyVotingResult?> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate,
        TenantId tenant,
        CancellationToken cancellationToken)
    {
        var currentVotingSession = await _ctx.VotingResults.AsNoTracking().SingleOrDefaultAsync(predicate, cancellationToken);
        return currentVotingSession;
    }

    public async Task<IEnumerable<IReadOnlyVotingResult>> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, TenantId tenant, Expression<Func<IReadOnlyVotingResult, object>> sortBy,
        int take,
        CancellationToken cancellationToken)
    {
        Func<IQueryable<IReadOnlyVotingResult>, IOrderedQueryable<IReadOnlyVotingResult>> sortFunction = take > 0 ? x => x.OrderBy(sortBy) : x => x.OrderByDescending(sortBy);
        
        var query = _ctx.VotingResults.Where(predicate);
        var currentVotingSession = await sortFunction.Invoke(query).Take(Math.Abs(take)).AsNoTracking().ToArrayAsync(cancellationToken);
        return currentVotingSession;
    }

    public async Task<IEnumerable<T>> Get<T>(Expression<Func<IReadOnlyVotingResult, bool>> predicate, Expression<Func<IReadOnlyVotingResult, T>> selector, TenantId tenant,
        CancellationToken cancellationToken)
        where T : class
    {
        var entities = await _ctx.VotingResults.Where(predicate).Select(selector).AsNoTracking().ToArrayAsync(cancellationToken);
        return entities;
    }
}