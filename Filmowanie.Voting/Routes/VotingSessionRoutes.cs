using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingSessionRoutes : IVotingSessionRoutes
{
    private readonly IDomainUserAccessor _userAccessor;
    private readonly ICurrentVotingSessionIdAccessor _currentVotingSessionIdAccessor;
    private readonly IMovieVotingSessionService _movieVotingSessionService;
    private readonly IVotingSessionMapper _movieVotingSessionMapper;

    private readonly IMoviesForVotingSessionEnricher _moviesForVotingSessionEnricher;
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IVoteService _voteService;

    public VotingSessionRoutes(IMoviesForVotingSessionEnricher moviesForVotingSessionEnricher, IFluentValidatorAdapterProvider validatorAdapterProvider, IVoteService voteService,IDomainUserAccessor userAccessor, ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor, IMovieVotingSessionService movieVotingSessionService, IVotingSessionMapper movieVotingSessionMapper)
    {
        _moviesForVotingSessionEnricher = moviesForVotingSessionEnricher;
        _validatorAdapterProvider = validatorAdapterProvider;
        _voteService = voteService;
        _userAccessor = userAccessor;
        _currentVotingSessionIdAccessor = currentVotingSessionIdAccessor;
        _movieVotingSessionService = movieVotingSessionService;
        _movieVotingSessionMapper = movieVotingSessionMapper;
    }

    public async Task<IResult> GetCurrentVotingSessionMoviesAsync(CancellationToken cancel)
    {
        var maybeCurrentUser = _userAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var merged = maybeCurrentVotingId.Merge(maybeCurrentUser);
        var maybeCurrentlyVotedMovies = await _movieVotingSessionService.GetCurrentlyVotedMoviesAsync(merged, cancel);
        var merged2 = maybeCurrentlyVotedMovies.Merge(maybeCurrentVotingId);
        var result = await _moviesForVotingSessionEnricher.EnrichWithPlaceholdersAsync(merged2, cancel);
     
        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> VoteAsync(VoteDTO dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<VoteDTO>();
        var maybeDto = validator.Validate(dto);
        var maybeCurrentUser = _userAccessor.GetDomainUser(maybeDto.AsVoid());
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
        var result = _movieVotingSessionMapper.Map(maybeNullableCurrentVotingId);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}