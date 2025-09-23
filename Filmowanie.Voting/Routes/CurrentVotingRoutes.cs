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
    private readonly ICurrentUserAccessor userAccessor;
    private readonly ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor;
    private readonly ICurrentVotingService currentVotingService;
    private readonly IVotingMappersComposite movieVotingMappersComposite;
    private readonly IVotingStateMapper votingStateMapper;
    private readonly ICurrentVotingStatusRetriever statusRetriever;

    private readonly IMoviesForVotingSessionEnricher moviesForVotingSessionEnricher;
    private readonly IFluentValidatorAdapterProvider validatorAdapterProvider;
    private readonly IVoteService voteService;

    public CurrentVotingRoutes(IMoviesForVotingSessionEnricher moviesForVotingSessionEnricher, IFluentValidatorAdapterProvider validatorAdapterProvider, IVoteService voteService, ICurrentUserAccessor userAccessor, ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor, ICurrentVotingService currentVotingService, IVotingMappersComposite movieVotingMappersComposite, IVotingStateMapper votingStateMapper, ICurrentVotingStatusRetriever statusRetriever)
    {
        this.moviesForVotingSessionEnricher = moviesForVotingSessionEnricher;
        this.validatorAdapterProvider = validatorAdapterProvider;
        this.voteService = voteService;
        this.userAccessor = userAccessor;
        this.currentVotingSessionIdAccessor = currentVotingSessionIdAccessor;
        this.currentVotingService = currentVotingService;
        this.movieVotingMappersComposite = movieVotingMappersComposite;
        this.votingStateMapper = votingStateMapper;
        this.statusRetriever = statusRetriever;
    }

    public async Task<IResult> GetCurrentVotingSessionMoviesAsync(CancellationToken cancel)
    {
        var maybeCurrentUser = this.userAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await this.currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeCurrentVotingId = this.currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeCurrentlyVotedMovies = await this.currentVotingService.GetCurrentlyVotedMoviesAsync(maybeCurrentVotingId, cancel);
        var maybeCurrentlyVotedMoviesWithVotes = await this.currentVotingService.GetCurrentlyVotedMoviesWithVotesAsync(maybeCurrentVotingId, cancel);
        var maybeDto = this.movieVotingMappersComposite.Map(maybeCurrentlyVotedMovies, maybeCurrentlyVotedMoviesWithVotes, maybeCurrentUser);
        var result = await this.moviesForVotingSessionEnricher.EnrichWithPlaceholdersAsync(maybeDto, maybeCurrentVotingId, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> VoteAsync(VoteDTO dto, CancellationToken cancel)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<VoteDTO>();
        var maybeDto = validator.Validate(dto);
        var maybeCurrentUser = this.userAccessor.GetDomainUser(maybeDto);
        var maybeNullableCurrentVotingId = await this.currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeCurrentVotingId = this.currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeVote = await this.voteService.VoteAsync(maybeCurrentUser, maybeCurrentVotingId, maybeDto, cancel);
        var result = maybeVote.Map(_ => new AknowledgedDTO { Message = "OK" });

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionStatus(CancellationToken cancel)
    {
        var maybeCurrentUser = this.userAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await this.currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeVotingStatus = await this.statusRetriever.GetCurrentVotingStatusAsync(maybeNullableCurrentVotingId, cancel);
        var result = this.votingStateMapper.Map(maybeVotingStatus, maybeNullableCurrentVotingId);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}
