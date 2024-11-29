using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Abstractions;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal class VotingSessionQueryRepository : IVotingSessionQueryRepository
{
    private readonly VotingResultsContext _ctx;

    public VotingSessionQueryRepository(VotingResultsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<VotingSessionId?> GetStartedEventsAsync(TenantId tenantId,
        CancellationToken cancellationToken)
    {
        var currentVotingSession = await _ctx.VotingResults.SingleOrDefaultAsync(x => x.Concluded == null, cancellationToken);

        if (currentVotingSession == null)
            return null;

        var result = Guid.Parse(currentVotingSession.Id);
        return new VotingSessionId(result);
    }
}

public interface IVotingSessionCommandRepository
{
    Task InsertAsync(IReadonlyVotingResult votingResult, CancellationToken cancellationToken);
}

internal sealed class VotingSessionCommandRepository : IVotingSessionCommandRepository
{
    private readonly VotingResultsContext _ctx;

    public VotingSessionCommandRepository(VotingResultsContext ctx)
    {
        _ctx = ctx;
    }

    public Task InsertAsync(IReadonlyVotingResult votingResult, CancellationToken cancellationToken)
    {
        var votingResultEntity = new VotingResult { Concluded = votingResult.Concluded, Created = votingResult.Created, id = votingResult.Id, TenantId = votingResult.TenantId };
        _ctx.VotingResults.Add(votingResultEntity);
        return _ctx.SaveChangesAsync(cancellationToken);
    }
}