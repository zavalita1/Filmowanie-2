using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal class VotingSessionQueryRepository : IVotingSessionQueryRepository
{
    private readonly VotingResultsContext _ctx;

    public VotingSessionQueryRepository(VotingResultsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<VotingSessionId> GetStartedEventsAsync(TenantId tenantId,
        CancellationToken cancellationToken)
    {
        var currentVotingSession = await _ctx.VotingResults.SingleAsync(x => x.Concluded == null, cancellationToken);
        var result = Guid.Parse(currentVotingSession.Id);
        return new VotingSessionId(result);
    }
}