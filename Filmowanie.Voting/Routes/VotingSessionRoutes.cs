using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Visitors;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingSessionRoutes : IVotingSessionRoutes
{
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetCurrentVotingSessionVisitor _currentVotingSessionVisitor;
    private readonly IGetCurrentVotingSessionStatusVisitor _currentVotingSessionStatusVisitor;
    private readonly IGetMoviesForVotingSessionVisitor _getMoviesForVotingSessionVisitor;
    private readonly IEnrichMoviesForVotingSessionWithPlaceholdersVisitor _enrichMoviesForVotingSessionWithPlaceholdersVisitor;
    private readonly IFluentValidatorAdapterFactory _validatorAdapterFactory;
    private readonly IVotingSessionStatusVisitor _votingSessionStatusVisitor;
    private readonly IVoteVisitor _voteVisitor;

    public VotingSessionRoutes(IUserIdentityVisitor userIdentityVisitor, IGetCurrentVotingSessionVisitor currentVotingSessionVisitor, IGetMoviesForVotingSessionVisitor getMoviesForVotingSessionVisitor, IEnrichMoviesForVotingSessionWithPlaceholdersVisitor enrichMoviesForVotingSessionWithPlaceholdersVisitor, IFluentValidatorAdapterFactory validatorAdapterFactory, IVoteVisitor voteVisitor, IGetCurrentVotingSessionStatusVisitor currentVotingSessionStatusVisitor, IVotingSessionStatusVisitor iVotingSessionStatusVisitor)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _currentVotingSessionVisitor = currentVotingSessionVisitor;
        _getMoviesForVotingSessionVisitor = getMoviesForVotingSessionVisitor;
        _enrichMoviesForVotingSessionWithPlaceholdersVisitor = enrichMoviesForVotingSessionWithPlaceholdersVisitor;
        _validatorAdapterFactory = validatorAdapterFactory;
        _voteVisitor = voteVisitor;
        _currentVotingSessionStatusVisitor = currentVotingSessionStatusVisitor;
        _votingSessionStatusVisitor = iVotingSessionStatusVisitor;
    }

    public async Task<IResult> GetCurrentVotingSessionMoviesAsync(CancellationToken cancel)
    {
        var userIdentity = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor);

        var votingSessionResult = await userIdentity.AcceptAsync(_currentVotingSessionVisitor, cancel);
        var result = await (await votingSessionResult
            .Merge(userIdentity)
            .AcceptAsync(_getMoviesForVotingSessionVisitor, cancel))
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

        var votingSessionResult = await userIdentity.AcceptAsync(_currentVotingSessionVisitor, cancel);
        var result = await userIdentity.Merge(votingSessionResult).Merge(validationResult).Flatten().AcceptAsync(_voteVisitor, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionStatus(CancellationToken cancel)
    {
        var result = (await OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor)
            .AcceptAsync(_currentVotingSessionStatusVisitor, cancel))
            .Accept(_votingSessionStatusVisitor);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}