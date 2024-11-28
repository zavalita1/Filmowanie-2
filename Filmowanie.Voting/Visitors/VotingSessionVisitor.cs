using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingSessionVisitor : IGetCurrentVotingSessionVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;

    public VotingSessionVisitor(IVotingSessionQueryRepository votingSessionQueryRepository)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
    }

    public async Task<OperationResult<VotingSessionId>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var sagaId = await _votingSessionQueryRepository.GetStartedEventsAsync(input.Result.Tenant, cancellationToken);
        return new OperationResult<VotingSessionId>(sagaId, null);
    }
}