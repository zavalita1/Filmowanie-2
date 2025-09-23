using System.Linq.Expressions;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories.Internal;

internal class VotingSessionQueryRepository : IVotingSessionQueryRepositoryInUserlessContext
{
    private readonly VotingResultsContext ctx;

    public VotingSessionQueryRepository(VotingResultsContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<IReadOnlyVotingResult?> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate,
        CancellationToken cancelToken)
    {
        var currentVotingSession = await this.ctx.VotingResults.AsNoTracking().SingleOrDefaultAsync(predicate, cancelToken);
        return currentVotingSession;
    } 

    public async Task<IEnumerable<IReadOnlyVotingResult>> GetAll(Expression<Func<IReadOnlyVotingResult, bool>> predicate, CancellationToken cancelToken)
    {
        return await this.ctx.VotingResults.AsNoTracking().Where(predicate).ToArrayAsync(cancelToken);
    }

    public async Task<IEnumerable<IReadOnlyVotingResult>> GetVotingResultAsync(Expression<Func<IReadOnlyVotingResult, bool>> predicate, Expression<Func<IReadOnlyVotingResult, object>> sortBy,
        int take,
        CancellationToken cancelToken)
    {
        Func<IQueryable<IReadOnlyVotingResult>, IOrderedQueryable<IReadOnlyVotingResult>> sortFunction = take > 0 ? x => x.OrderBy(sortBy) : x => x.OrderByDescending(sortBy);
        
        var query = this.ctx.VotingResults.Where(predicate);
        var currentVotingSession = await sortFunction.Invoke(query).Take(Math.Abs(take)).AsNoTracking().ToArrayAsync(cancelToken);
        return currentVotingSession;
    }
}