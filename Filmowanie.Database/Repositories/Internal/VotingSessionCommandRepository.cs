using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories.Internal;

internal sealed class VotingSessionCommandRepository : IVotingSessionCommandRepository
{
    private readonly VotingResultsContext _ctx;

    public VotingSessionCommandRepository(VotingResultsContext ctx)
    {
        _ctx = ctx;
    }

    public Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancelToken)
    {
        var votingResultEntity = votingResult.AsMutable();
        _ctx.VotingResults.Add(votingResultEntity);
        return _ctx.SaveChangesAsync(cancelToken);
    }

    public async Task UpdateAsync(string id, Action<VotingResult> updateAction, CancellationToken cancelToken)
    {
        var votingResultEntity = await _ctx.VotingResults.SingleAsync(x => x.id == id, cancelToken);
        updateAction.Invoke(votingResultEntity);

        await _ctx.SaveChangesAsync(cancelToken);
    }
}