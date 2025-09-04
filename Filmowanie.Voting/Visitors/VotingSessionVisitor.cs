using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingSessionService : IVotingSessionService
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly ILogger<VotingSessionService> _log;

    public VotingSessionService(IVotingSessionQueryRepository votingSessionQueryRepository, ILogger<VotingSessionService> log)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _log = log;
    }

    public Task<OperationResult<IReadOnlyVotingResult?>> GetCurrentVotingSession(OperationResult<DomainUser> maybeCurrentUser, CancellationToken cancellationToken) =>
        maybeCurrentUser.AcceptAsync(GetCurrentVotingSession, _log, cancellationToken);

    public Task<OperationResult<VotingSessionId?>> GetCurrentVotingSessionId(OperationResult<DomainUser> maybeCurrentUser, CancellationToken cancellationToken) =>
        maybeCurrentUser.AcceptAsync(GetCurrentVotingSessionId, _log, cancellationToken);

    public OperationResult<VotingSessionId> GetRequiredCurrentVotingSessionId(OperationResult<VotingSessionId?> maybeCurrentVotingSessionId) =>
        maybeCurrentVotingSessionId.Accept(GetRequiredCurrentVotingSessionId, _log);

    public async Task<OperationResult<IReadOnlyVotingResult?>> GetCurrentVotingSession(DomainUser currentUser, CancellationToken cancellationToken)
    {
        var currentVotingResults = await _votingSessionQueryRepository.Get(x => x.Concluded == null, currentUser.Tenant, cancellationToken);
        return currentVotingResults.ToOperationResult();
    }

    public async Task<OperationResult<VotingSessionId?>> GetCurrentVotingSessionId(DomainUser currentUser, CancellationToken cancellationToken)
    {
        var votingSession = await GetCurrentVotingSession(currentUser, cancellationToken);

        if (votingSession.Result == null)
            return default(VotingSessionId?).ToOperationResult(); // no current voting = voting has not been started yet.

        return votingSession.Pluck(x => Guid.Parse(x.id)).Pluck(x => new VotingSessionId(x) as VotingSessionId?);
    }

    private OperationResult<VotingSessionId> GetRequiredCurrentVotingSessionId(VotingSessionId? input)
    {
        if (!input.HasValue)
            return new OperationResult<VotingSessionId>(default, new Error("Voting has not started yet!", ErrorType.IncomingDataIssue));

        return input.Value.ToOperationResult();
    }
}

internal sealed class VotingSessionIdQueryVisitor : IGetCurrentVotingSessionStatusVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;

    public VotingSessionIdQueryVisitor(IVotingSessionQueryRepository votingSessionQueryRepository, ILogger<VotingSessionIdQueryVisitor> log)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
    }

    public async Task<OperationResult<VotingSessionId?>> SignUp(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var currentVotingResults = await _votingSessionQueryRepository.Get(x => x.Concluded == null, input.Result.Tenant, cancellationToken);

        if (currentVotingResults == null)
            return new OperationResult<VotingSessionId?>(null, null);

        var correlationId = Guid.Parse(currentVotingResults.id);
        var votingSessionId = new VotingSessionId(correlationId);
        return new OperationResult<VotingSessionId?>(votingSessionId, null);
    }

    async Task<OperationResult<VotingState>> IOperationAsyncVisitor<DomainUser, VotingState>.SignUp(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var sagaId = await _votingSessionQueryRepository.Get(x => x.Concluded == null, input.Result.Tenant, cancellationToken);
        var state = sagaId == null ? VotingState.Results : VotingState.Voting;
        return new OperationResult<VotingState>(state, null);
    }

    

    public ILogger Log => _log;
}
