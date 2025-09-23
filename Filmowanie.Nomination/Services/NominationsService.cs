using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Services;

// TODO UTs
internal sealed class NominationsService : INominationsService
{
    private readonly IRequestClient<NominationsRequestedEvent> getNominationsRequestClient;
    private readonly IMovieCommandRepository movieCommandRepository;
    private readonly IMovieDomainRepository movieQueryRepository;
    private readonly IBus bus;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ILogger<NominationsService> log;

    public NominationsService(IRequestClient<NominationsRequestedEvent> getNominationsRequestClient, ILogger<NominationsService> log, IMovieCommandRepository movieCommandRepository, IMovieDomainRepository movieQueryRepository, IBus bus, IDateTimeProvider dateTimeProvider)
    {
        this.getNominationsRequestClient = getNominationsRequestClient;
        this.log = log;
        this.movieCommandRepository = movieCommandRepository;
        this.movieQueryRepository = movieQueryRepository;
        this.bus = bus;
        this.dateTimeProvider = dateTimeProvider;
    }

    public Task<Maybe<CurrentNominationsData>> GetNominationsAsync(Maybe<VotingSessionId> maybeId, CancellationToken cancelToken) => maybeId.AcceptAsync(GetNominations, this.log, cancelToken);

    public Task<Maybe<AknowledgedNominationDTO>> NominateAsync(Maybe<IReadOnlyMovieEntity> maybeMovie, Maybe<DomainUser> maybeUser, Maybe<VotingSessionId> maybeCurrentVotingId,
        CancellationToken cancelToken) => maybeMovie.Merge(maybeUser).Merge(maybeCurrentVotingId).Flatten().AcceptAsync(NominateAsync, this.log, cancelToken);

    public Task<Maybe<AknowledgedNominationDTO>> ResetNominationAsync(Maybe<string> maybeMovieId, Maybe<DomainUser> maybeUser, Maybe<VotingSessionId> maybeVotingId,
        CancellationToken cancelToken) => maybeMovieId.Merge(maybeUser).Merge(maybeVotingId).Flatten().AcceptAsync(ResetNominationAsync, this.log, cancelToken);

    private async Task<Maybe<CurrentNominationsData>> GetNominations(VotingSessionId id, CancellationToken cancelToken)
    {
        var nominationsRequested = new NominationsRequestedEvent(id);
        var nominationsResponse = await this.getNominationsRequestClient.GetResponse<CurrentNominationsResponse>(nominationsRequested, cancelToken, TimeSpan.FromSeconds(3));
        var nominations = nominationsResponse.Message;
        var result = new CurrentNominationsData(id, nominations.CorrelationId, nominations.Nominations);
        return result.AsMaybe();
    }

    private async Task<Maybe<AknowledgedNominationDTO>> ResetNominationAsync((string MovieId, DomainUser User, VotingSessionId VotingSessionId) input, CancellationToken cancelToken)
    {
        var (movieId, user, votingSessionId) = input;
        try
        {
            var movieResult = await this.movieCommandRepository.MarkMovieAsRejectedAsync(movieId, cancelToken);
            var movie = new EmbeddedMovie { id = movieResult.id, MovieCreationYear = movieResult.CreationYear, Name = movieResult.Name };

            await this.bus.Publish(new RemoveMovieEvent(votingSessionId, movie, user), cancelToken);
            var decade = movie.MovieCreationYear.ToDecade().ToString();
            return new AknowledgedNominationDTO { Message = "OK", Decade = decade }.AsMaybe();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error during soft-deleting movie");
            return new Error<AknowledgedNominationDTO>("Error during soft-deleting movie", ErrorType.InvalidState);
        }
    }

    public async Task<Maybe<AknowledgedNominationDTO>> NominateAsync((IReadOnlyMovieEntity Movie, DomainUser User, VotingSessionId VotingSessionId) input, CancellationToken cancelToken)
    {
        var (movie, user, votingSessionId) = input;
        var embeddedMovie = new EmbeddedMovie { id = movie.id, MovieCreationYear = movie.CreationYear, Name = movie.Name };
        var existingMovie = await movieQueryRepository.GetByNameAsync(movie.Name, movie.CreationYear, cancelToken);
        if (existingMovie.Error.HasValue)
            return existingMovie.Error.Value.ChangeResultType<IReadOnlyMovieEntity?, AknowledgedNominationDTO>();

        if (existingMovie.Result != null)
        {
            if (existingMovie.Result.IsRejected == true)
                return new Error<AknowledgedNominationDTO>("This movie had already been nominated and was rejected.", ErrorType.IncomingDataIssue);

            var canBeNominatedAgainEvents = await movieQueryRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(cancelToken);
            var canBeNominatedAgainEventForMovie = canBeNominatedAgainEvents.Where(x => x.Movie.Name == movie.Name && x.Movie.MovieCreationYear == movie.CreationYear).ToArray();
            var nominatedAgainEvents = await movieQueryRepository.GetMovieNominatedEventsAsync(movie.Name, movie.CreationYear, cancelToken);
           
            var maxDateCanBeNominatedAgain = canBeNominatedAgainEventForMovie.OrderByDescending(x => x.Created).FirstOrDefault()?.Created;
            var maxDateNominatedAgain = nominatedAgainEvents.OrderByDescending(x => x.Created).FirstOrDefault()?.Created ?? DateTime.MinValue;
            var canBeNominatedAgain = maxDateCanBeNominatedAgain > maxDateNominatedAgain;

            if (!canBeNominatedAgain)
                return new Error<AknowledgedNominationDTO>("This movie had already been watched or is still waiting until it can be nominated again!", ErrorType.IncomingDataIssue);

            var movieId = canBeNominatedAgainEventForMovie[0].Movie.id;
            embeddedMovie.id = movieId;
            var nominatedEvent = new NominatedEventRecord(embeddedMovie, "nominated-event-" + movieId, this.dateTimeProvider.Now, user.Tenant.Id, user.Id);
            await this.movieCommandRepository.InsertNominatedAsync(nominatedEvent, cancelToken);
            await this.movieCommandRepository.UpdateMovieAsync(movieId, movie.PosterUrl, cancelToken);
        }
        else
        {
            var nominatedEvent = new NominatedEventRecord(embeddedMovie, "nominated-event-" + movie.id, this.dateTimeProvider.Now, user.Tenant.Id, user.Id);
            await this.movieCommandRepository.InsertMovieAsync(movie, cancelToken);
            await this.movieCommandRepository.InsertNominatedAsync(nominatedEvent, cancelToken);
        }

        var addMovieEvent = new AddMovieEvent(votingSessionId, embeddedMovie, user, movie.CreationYear.ToDecade());
        await this.bus.Publish(addMovieEvent, cancelToken);

        var dto = new AknowledgedNominationDTO { Decade = movie.CreationYear.ToDecade().ToString()[1..], Message = "OK" };
        return dto.AsMaybe();
    }
}