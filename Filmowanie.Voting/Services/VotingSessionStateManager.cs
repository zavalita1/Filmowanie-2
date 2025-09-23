using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class VotingSessionStateManager : IVotingStateManager
{
    private readonly ILogger<VotingSessionStateManager> log;
    private readonly IBus bus;
    private readonly IVotingResultsRepository votingSessionQueryRepository;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IGuidProvider guidProvider;
    private readonly ICurrentVotingSessionCacheService currentVotingSessionCacheService;

    public VotingSessionStateManager(ILogger<VotingSessionStateManager> log, IBus bus, IVotingResultsRepository votingSessionQueryRepository, IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider, ICurrentVotingSessionCacheService currentVotingSessionCacheService)
    {
        this.log = log;
        this.bus = bus;
        this.votingSessionQueryRepository = votingSessionQueryRepository;
        this.dateTimeProvider = dateTimeProvider;
        this.guidProvider = guidProvider;
        this.currentVotingSessionCacheService = currentVotingSessionCacheService;
    }

    public Task<Maybe<VoidResult>> ConcludeVotingAsync(Maybe<VotingSessionId> maybeVotingId, Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken) =>
        maybeVotingId.Merge(maybeCurrentUser).AcceptAsync(ConcludeVotingAsync, this.log, cancelToken);

    public Task<Maybe<VotingSessionId>> StartNewVotingAsync(Maybe<DomainUser> input, CancellationToken cancelToken) =>
        input.AcceptAsync(StartNewVotingAsync, this.log, cancelToken);

    public Task<Maybe<VoidResult>> ResumeVotingAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken) => input.AcceptAsync(ResumeVotingAsync, this.log, cancelToken);

    private async Task<Maybe<VoidResult>> ResumeVotingAsync(VotingSessionId votingSessionId, CancellationToken cancelToken)
    {
        var message = new ResumeVotingEvent(votingSessionId);
        await this.bus.Publish(message, cancelToken);
        return VoidResult.Void;
    }

    private async Task<Maybe<VotingSessionId>> StartNewVotingAsync(DomainUser input, CancellationToken cancelToken)
    {
        var lastConcludedVotingSession = await this.votingSessionQueryRepository
            .GetLastNVotingResultsAsync(1, cancelToken);
        var correlationId = this.guidProvider.NewGuid();

        if (lastConcludedVotingSession.Error.HasValue)
            return lastConcludedVotingSession.Error.Value.ChangeResultType<IEnumerable<IReadOnlyVotingResult>, VotingSessionId>();

        if (!lastConcludedVotingSession.Result!.Any())
            return new Error<VotingSessionId>("Previous voting has not concluded!", ErrorType.InvalidState);

        var lastVotingResult = lastConcludedVotingSession.Result!.Single();

        if (!Guid.TryParse(lastVotingResult.id, out var lastVotingId))
            return new Error<VotingSessionId>("Can't parse last voting id!", ErrorType.InvalidState);

        var lastVotingResultId = new VotingSessionId(lastVotingId);
        await this.bus.Publish(new ResultsConfirmedEvent(lastVotingResultId, input.Tenant), cancelToken);
        
        var votingSessionId = await PrepareAndPublishNewVoteEventAsync(input, lastVotingResult, correlationId, cancelToken);

        return new Maybe<VotingSessionId>(votingSessionId, null);
    }

    private async Task<VotingSessionId> PrepareAndPublishNewVoteEventAsync(DomainUser input, IReadOnlyVotingResult lastVotingResult, Guid correlationId, CancellationToken cancelToken)
    {
        try
        {
            var movies = GetEmbeddedMovies(lastVotingResult, correlationId, out var nominationsData, out var votingSessionId);
            this.currentVotingSessionCacheService.InvalidateCache(input.Tenant);
            var @event = new StartVotingEvent(votingSessionId, movies, nominationsData, this.dateTimeProvider.Now, input.Tenant);
            await this.bus.Publish(@event, cancelToken);
            return votingSessionId;
        }
        catch (Exception ex)
        {
            this.log.LogCritical(ex, "Error during sending voting event! This needs manual correction!");
            throw;
        }
    }

    private EmbeddedMovie[] GetEmbeddedMovies(IReadOnlyVotingResult lastVotingResult, Guid correlationId, out NominationData[] nominationsData, out VotingSessionId votingSessionId)
    {
        EmbeddedMovie[] movies = [];
        try
        {
            var moviesGoingByeByeIds = lastVotingResult.MoviesGoingByeBye.Select(x => x.id).ToArray();
            movies = lastVotingResult.Movies.Select(x => new EmbeddedMovie { id = x.Movie.id, Name = x.Movie.Name, MovieCreationYear = x.Movie.MovieCreationYear })
                .Where(x => x.id != lastVotingResult.Winner!.Movie.id).Where(x => !moviesGoingByeByeIds.Contains(x.id)).ToArray();
            nominationsData = lastVotingResult.UsersAwardedWithNominations.Select(x => new NominationData
            {
                Concluded = null,
                User = new NominationDataEmbeddedUser { DisplayName = x.User.Name, Id = x.User.id },
                Year = x.Decade
            }).ToArray();

        }
        catch (Exception ex)
        {
            nominationsData = [];
            this.log.LogCritical(ex, "Error occurred during voting start event preparation. It will resort to empty values and will need manual correction!");
        }

        votingSessionId = new VotingSessionId(correlationId);
        return movies;
    }

    private async Task<Maybe<VoidResult>> ConcludeVotingAsync((VotingSessionId, DomainUser) input, CancellationToken cancelToken)
    {
        var @event = new ConcludeVotingEvent(input.Item1, input.Item2.Tenant);
        await this.bus.Publish(@event, cancelToken);
        return VoidResult.Void;
    }
}