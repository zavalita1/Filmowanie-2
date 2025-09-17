using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

// TODO UTs
internal sealed class CurrentVotingRoutes : IVotingSessionRoutes
{
    private readonly ICurrentUserAccessor _userAccessor;
    private readonly ICurrentVotingSessionIdAccessor _currentVotingSessionIdAccessor;
    private readonly ICurrentVotingService _currentVotingService;
    private readonly IVotingMappersComposite _movieVotingMappersComposite;
    private readonly IVotingStateMapper _votingStateMapper;
    private readonly ICurrentVotingStatusRetriever _statusRetriever;

    private readonly IMoviesForVotingSessionEnricher _moviesForVotingSessionEnricher;
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IVoteService _voteService;

    public CurrentVotingRoutes(IMoviesForVotingSessionEnricher moviesForVotingSessionEnricher, IFluentValidatorAdapterProvider validatorAdapterProvider, IVoteService voteService, ICurrentUserAccessor userAccessor, ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor, ICurrentVotingService currentVotingService, IVotingMappersComposite movieVotingMappersComposite, IVotingStateMapper votingStateMapper, ICurrentVotingStatusRetriever statusRetriever)
    {
        _moviesForVotingSessionEnricher = moviesForVotingSessionEnricher;
        _validatorAdapterProvider = validatorAdapterProvider;
        _voteService = voteService;
        _userAccessor = userAccessor;
        _currentVotingSessionIdAccessor = currentVotingSessionIdAccessor;
        _currentVotingService = currentVotingService;
        _movieVotingMappersComposite = movieVotingMappersComposite;
        _votingStateMapper = votingStateMapper;
        _statusRetriever = statusRetriever;
    }

    public async Task<IResult> GetCurrentVotingSessionMoviesAsync(CancellationToken cancel)
    {
        var maybeCurrentUser = _userAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeCurrentlyVotedMovies = await _currentVotingService.GetCurrentlyVotedMoviesAsync(maybeCurrentVotingId, cancel);
        var maybeCurrentlyVotedMoviesWithVotes = await _currentVotingService.GetCurrentlyVotedMoviesWithVotesAsync(maybeCurrentVotingId, cancel);
        var merged = maybeCurrentlyVotedMovies.Merge(maybeCurrentlyVotedMoviesWithVotes).Merge(maybeCurrentUser).Flatten();
        var maybeDto = _movieVotingMappersComposite.Map(merged);
        var merged2 = maybeDto.Merge(maybeCurrentVotingId);
        var result = await _moviesForVotingSessionEnricher.EnrichWithPlaceholdersAsync(merged2, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> VoteAsync(VoteDTO dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<VoteDTO>();
        var maybeDto = validator.Validate(dto);
        var maybeCurrentUser = _userAccessor.GetDomainUser(maybeDto);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var merged = maybeCurrentUser.Merge(maybeCurrentVotingId).Merge(maybeDto).Flatten();
        var maybeVote = await _voteService.VoteAsync(merged, cancel);
        var result = maybeVote.Map(_ => new AknowledgedDTO { Message = "OK" });

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionStatus(CancellationToken cancel)
    {
        var maybeCurrentUser = _userAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeVotingStatus = await _statusRetriever.GetCurrentVotingStatusAsync(maybeNullableCurrentVotingId,  cancel);
        var result = _votingStateMapper.Map(maybeVotingStatus.Merge(maybeNullableCurrentVotingId));

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}
