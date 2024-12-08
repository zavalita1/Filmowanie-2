using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingSessionRoutes : IVotingSessionRoutes
{
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetCurrentVotingSessionIdVisitor _currentVotingSessionIdVisitor;
    private readonly IGetCurrentVotingSessionStatusVisitor _currentVotingSessionStatusVisitor;
    private readonly IGetMoviesForVotingSessionVisitor _getMoviesForVotingSessionVisitor;
    private readonly IRequireCurrentVotingSessionIdVisitor _requireCurrentVotingSessionIdVisitor;
    private readonly IEnrichMoviesForVotingSessionWithPlaceholdersVisitor _enrichMoviesForVotingSessionWithPlaceholdersVisitor;
    private readonly IFluentValidatorAdapterFactory _validatorAdapterFactory;
    private readonly IVotingSessionStatusMapperVisitor _iVotingSessionStatusMapperVisitor;
    private readonly IAknowledgedMapperVisitor _aknowledgedMapperVisitor;
    private readonly IVoteVisitor _voteVisitor;

    public VotingSessionRoutes(IUserIdentityVisitor userIdentityVisitor, IGetCurrentVotingSessionIdVisitor currentVotingSessionIdVisitor, IGetMoviesForVotingSessionVisitor getMoviesForVotingSessionVisitor, IEnrichMoviesForVotingSessionWithPlaceholdersVisitor enrichMoviesForVotingSessionWithPlaceholdersVisitor, IFluentValidatorAdapterFactory validatorAdapterFactory, IVoteVisitor voteVisitor, IGetCurrentVotingSessionStatusVisitor currentVotingSessionStatusVisitor, IVotingSessionStatusMapperVisitor iIVotingSessionStatusMapperVisitor, IAknowledgedMapperVisitor aknowledgedMapperVisitor, IRequireCurrentVotingSessionIdVisitor requireCurrentVotingSessionIdVisitor)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _currentVotingSessionIdVisitor = currentVotingSessionIdVisitor;
        _getMoviesForVotingSessionVisitor = getMoviesForVotingSessionVisitor;
        _enrichMoviesForVotingSessionWithPlaceholdersVisitor = enrichMoviesForVotingSessionWithPlaceholdersVisitor;
        _validatorAdapterFactory = validatorAdapterFactory;
        _voteVisitor = voteVisitor;
        _currentVotingSessionStatusVisitor = currentVotingSessionStatusVisitor;
        _iVotingSessionStatusMapperVisitor = iIVotingSessionStatusMapperVisitor;
        _aknowledgedMapperVisitor = aknowledgedMapperVisitor;
        _requireCurrentVotingSessionIdVisitor = requireCurrentVotingSessionIdVisitor;
    }

    public async Task<IResult> GetCurrentVotingSessionMoviesAsync(CancellationToken cancel)
    {
        var userIdentity = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor);

        var votingSessionResult = (await userIdentity
                .AcceptAsync(_currentVotingSessionIdVisitor, cancel))
                .Accept(_requireCurrentVotingSessionIdVisitor);

        var result = await (await votingSessionResult
            .Merge(userIdentity)
            .AcceptAsync(_getMoviesForVotingSessionVisitor, cancel))
            .Merge(votingSessionResult)
            .AcceptAsync(_enrichMoviesForVotingSessionWithPlaceholdersVisitor, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> VoteAsync(VoteDTO dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<VoteDTO>();
        var validationResult = validator.Validate(dto);

        var userIdentity = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor);

        var votingSessionResult = (await userIdentity
                .AcceptAsync(_currentVotingSessionIdVisitor, cancel))
                .Accept(_requireCurrentVotingSessionIdVisitor);

        var result = (await userIdentity
            .Merge(votingSessionResult)
            .Merge(validationResult)
            .Flatten()
            .AcceptAsync(_voteVisitor, cancel)).Accept(_aknowledgedMapperVisitor);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionStatus(CancellationToken cancel)
    {
        var result = (await OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor)
            .AcceptAsync(_currentVotingSessionStatusVisitor, cancel))
            .Accept(_iVotingSessionStatusMapperVisitor);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}