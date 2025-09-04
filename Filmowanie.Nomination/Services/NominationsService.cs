using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Services;

internal sealed class NominationsService : INominationsService
{
    private readonly IRequestClient<NominationsRequestedEvent> _getNominationsRequestClient;
    private readonly IFilmwebPathResolver _filmwebPathResolver;
    private readonly IFilmwebHandler _filmwebHandler;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly IBus _bus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<NominationsService> _log;

    public NominationsService(IRequestClient<NominationsRequestedEvent> getNominationsRequestClient, ILogger<NominationsService> log, IFilmwebPathResolver filmwebPathResolver, IFilmwebHandler filmwebHandler, IMovieCommandRepository movieCommandRepository, IMovieQueryRepository movieQueryRepository, IBus bus, IDateTimeProvider dateTimeProvider)
    {
        _getNominationsRequestClient = getNominationsRequestClient;
        _log = log;
        _filmwebPathResolver = filmwebPathResolver;
        _filmwebHandler = filmwebHandler;
        _movieCommandRepository = movieCommandRepository;
        _movieQueryRepository = movieQueryRepository;
        _bus = bus;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Maybe<CurrentNominationsData>> GetNominationsAsync(Maybe<VotingSessionId> maybeId, CancellationToken cancelToken) => maybeId.AcceptAsync(GetNominations, _log, cancelToken);

    public Task<Maybe<AknowledgedNominationDTO>> NominateAsync(Maybe<(NominationDTO Dto, DomainUser User, CurrentNominationsData CurrentNominations)> input,
        CancellationToken cancellationToken) => input.AcceptAsync(NominateAsync, _log, cancellationToken);

    public Task<Maybe<AknowledgedNominationDTO>> RemoveMovieAsync(Maybe<(string MovieId, DomainUser User, VotingSessionId VotingSessionId)> input,
        CancellationToken cancellationToken) => input.AcceptAsync(RemoveMovieAsync, _log, cancellationToken);

    private async Task<Maybe<CurrentNominationsData>> GetNominations(VotingSessionId id, CancellationToken cancelToken)
    {
        var nominationsRequested = new NominationsRequestedEvent(id);
        var nominationsResponse = await _getNominationsRequestClient.GetResponse<CurrentNominationsResponse>(nominationsRequested, cancelToken);
        var nominations = nominationsResponse.Message;
        var result = new CurrentNominationsData(id, nominations.CorrelationId, nominations.Nominations);
        return result.AsMaybe();
    }

    private async Task<Maybe<AknowledgedNominationDTO>> RemoveMovieAsync((string MovieId, DomainUser User, VotingSessionId VotingSessionId) input, CancellationToken cancellationToken)
    {
        var (movieId, user, votingSessionId) = input;
        var movies = await _movieQueryRepository.GetMoviesAsync(x => x.id == movieId, user.Tenant, cancellationToken);

        if (!movies.Any())
            return new Error("No such movie found!", ErrorType.IncomingDataIssue).AsMaybe<AknowledgedNominationDTO>();

        var movieEntity = movies.Single();
        var movie = new EmbeddedMovie { id = movieEntity.id, MovieCreationYear = movieEntity.CreationYear, Name = movieEntity.Name };

        await _bus.Publish(new RemoveMovieEvent(votingSessionId, movie, user), cancellationToken);
        return new AknowledgedNominationDTO { Message = "OK" }.AsMaybe();
    }

    public async Task<Maybe<AknowledgedNominationDTO>> NominateAsync((NominationDTO Dto, DomainUser User, CurrentNominationsData CurrentNominations) input, CancellationToken cancellationToken)
    {
        var metadata = _filmwebPathResolver.GetMetadata(input.Dto.MovieFilmwebUrl);
        var user = input.User;
        var movie = await _filmwebHandler.GetMovie(metadata, user.Tenant, input.Dto.PosterUrl, cancellationToken);
        var embeddedMovie = new EmbeddedMovie { id = movie.id, MovieCreationYear = movie.CreationYear, Name = movie.Name };

        if (movie.Genres.Contains("horror", StringComparer.OrdinalIgnoreCase))
            return new Error("Horrors are not allowed. Nice try motherfucker.", ErrorType.IncomingDataIssue).AsMaybe<AknowledgedNominationDTO>();

        var allowedDecades = input.CurrentNominations.NominationData.Where(x => x.User.Id == user.Id).Select(x => x.Year).ToArray();
        if (allowedDecades.All(x => x != movie.CreationYear.ToDecade()))
            return new Error($"This movie is not from decades: {string.Join(',', allowedDecades)}!", ErrorType.IncomingDataIssue).AsMaybe<AknowledgedNominationDTO>();

        var existingMovies = await _movieQueryRepository.GetMoviesAsync(x => x.Name == movie.Name, user.Tenant, cancellationToken); // TODO address movies with nonunique names
        if (existingMovies.Any())
        {
            var canBeNominatedAgainEvents = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(x => x.Movie.Name == movie.Name, user.Tenant, cancellationToken);
            var nominatedAgainEvents = await _movieQueryRepository.GetMoviesNominatedAgainEventsAsync(x => x.Movie.Name == movie.Name, user.Tenant, cancellationToken);

            var maxDateCanBeNominatedAgain = canBeNominatedAgainEvents.OrderByDescending(x => x.Created).FirstOrDefault()?.Created;
            var maxDateNominatedAgain = nominatedAgainEvents.OrderByDescending(x => x.Created).FirstOrDefault()?.Created ?? DateTime.MinValue;
            var canBeNominatedAgain = maxDateCanBeNominatedAgain > maxDateNominatedAgain;

            if (!canBeNominatedAgain)
                return new Error("This movie had already been watched or is still waiting until it can be nominated again!", ErrorType.IncomingDataIssue).AsMaybe<AknowledgedNominationDTO>();

            var movieId = canBeNominatedAgainEvents[0].Movie.id;
            embeddedMovie = new EmbeddedMovie { id = movieId, MovieCreationYear = movie.CreationYear, Name = movie.Name };
            var nominatedAgainEvent = new NominatedMovieAgainEventRecord(embeddedMovie, "nominated-again-event-" + movieId, _dateTimeProvider.Now, user.Tenant.Id);
            await _movieCommandRepository.InsertNominatedAgainAsync(nominatedAgainEvent, cancellationToken);
            await _movieCommandRepository.UpdateMovieAsync(movieId, movie.PosterUrl, cancellationToken);
        }
        else
        {
            await _movieCommandRepository.InsertMovieAsync(movie, cancellationToken);
        }

        var addMovieEvent = new AddMovieEvent(input.CurrentNominations.VotingSessionId, embeddedMovie, user, movie.CreationYear.ToDecade());
        await _bus.Publish(addMovieEvent, cancellationToken);

        var dto = new AknowledgedNominationDTO { Decade = movie.CreationYear.ToDecade().ToString()[1..], Message = "OK" };
        return dto.AsMaybe();
    }
}