using Filmowanie.Abstractions;
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
using Filmowanie.Database.Repositories;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Services;

// TODO UTs
internal sealed class NominationsService : INominationsService
{
    private readonly IRequestClient<NominationsRequestedEvent> _getNominationsRequestClient;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private readonly IMovieDomainRepository _movieQueryRepository;
    private readonly IBus _bus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<NominationsService> _log;

    public NominationsService(IRequestClient<NominationsRequestedEvent> getNominationsRequestClient, ILogger<NominationsService> log, IMovieCommandRepository movieCommandRepository, IMovieDomainRepository movieQueryRepository, IBus bus, IDateTimeProvider dateTimeProvider)
    {
        _getNominationsRequestClient = getNominationsRequestClient;
        _log = log;
        _movieCommandRepository = movieCommandRepository;
        _movieQueryRepository = movieQueryRepository;
        _bus = bus;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Maybe<CurrentNominationsData>> GetNominationsAsync(Maybe<VotingSessionId> maybeId, CancellationToken cancelToken) => maybeId.AcceptAsync(GetNominations, _log, cancelToken);

    public Task<Maybe<AknowledgedNominationDTO>> NominateAsync(Maybe<(IReadOnlyMovieEntity Movie, DomainUser User, VotingSessionId VotingSessionId)> input,
        CancellationToken cancelToken) => input.AcceptAsync(NominateAsync, _log, cancelToken);

    public Task<Maybe<AknowledgedNominationDTO>> ResetNominationAsync(Maybe<(string MovieId, DomainUser User, VotingSessionId VotingSessionId)> input,
        CancellationToken cancelToken) => input.AcceptAsync(ResetNominationAsync, _log, cancelToken);

    private async Task<Maybe<CurrentNominationsData>> GetNominations(VotingSessionId id, CancellationToken cancelToken)
    {
        var nominationsRequested = new NominationsRequestedEvent(id);
        var nominationsResponse = await _getNominationsRequestClient.GetResponse<CurrentNominationsResponse>(nominationsRequested, cancelToken, TimeSpan.FromSeconds(3));
        var nominations = nominationsResponse.Message;
        var result = new CurrentNominationsData(id, nominations.CorrelationId, nominations.Nominations);
        return result.AsMaybe();
    }

    private async Task<Maybe<AknowledgedNominationDTO>> ResetNominationAsync((string MovieId, DomainUser User, VotingSessionId VotingSessionId) input, CancellationToken cancelToken)
    {
        var (movieId, user, votingSessionId) = input;
        try
        {
            var movieResult = await _movieCommandRepository.MarkMovieAsRejectedAsync(movieId, cancelToken);
            var movie = new EmbeddedMovie { id = movieResult.id, MovieCreationYear = movieResult.CreationYear, Name = movieResult.Name };

            await _bus.Publish(new RemoveMovieEvent(votingSessionId, movie, user), cancelToken);
            return new AknowledgedNominationDTO { Message = "OK" }.AsMaybe();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error during soft-deleting movie");
            return new Error<AknowledgedNominationDTO>("Error during soft-deleting movie", ErrorType.InvalidState);
        }
    }

    public async Task<Maybe<AknowledgedNominationDTO>> NominateAsync((IReadOnlyMovieEntity Movie, DomainUser User, VotingSessionId VotingSessionId) input, CancellationToken cancelToken)
    {
        var (movie, user, votingSessionId) = input;
        var embeddedMovie = new EmbeddedMovie { id = movie.id, MovieCreationYear = movie.CreationYear, Name = movie.Name };
        var existingMovie = await _movieQueryRepository.GetByNameAsync(movie.Name, movie.CreationYear, cancelToken);
        if (existingMovie.Error.HasValue)
            return existingMovie.Error.Value.ChangeResultType<IReadOnlyMovieEntity?, AknowledgedNominationDTO>();

        if (existingMovie.Result != null)
        {
            if (existingMovie.Result.IsRejected == true)
                return new Error<AknowledgedNominationDTO>("This movie had already been nominated and was rejected.", ErrorType.IncomingDataIssue);

            var canBeNominatedAgainEvents = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(cancelToken);
            var canBeNominatedAgainEventForMovie = canBeNominatedAgainEvents.Where(x => x.Movie.Name == movie.Name && x.Movie.MovieCreationYear == movie.CreationYear).ToArray();
            var nominatedAgainEvents = await _movieQueryRepository.GetMovieNominatedEventsAsync(movie.Name, movie.CreationYear, cancelToken);
           
            var maxDateCanBeNominatedAgain = canBeNominatedAgainEventForMovie.OrderByDescending(x => x.Created).FirstOrDefault()?.Created;
            var maxDateNominatedAgain = nominatedAgainEvents.OrderByDescending(x => x.Created).FirstOrDefault()?.Created ?? DateTime.MinValue;
            var canBeNominatedAgain = maxDateCanBeNominatedAgain > maxDateNominatedAgain;

            if (!canBeNominatedAgain)
                return new Error<AknowledgedNominationDTO>("This movie had already been watched or is still waiting until it can be nominated again!", ErrorType.IncomingDataIssue);

            var movieId = canBeNominatedAgainEventForMovie[0].Movie.id;
            embeddedMovie.id = movieId;
            var nominatedEvent = new NominatedEventRecord(embeddedMovie, "nominated-event-" + movieId, _dateTimeProvider.Now, user.Tenant.Id, user.Id);
            await _movieCommandRepository.InsertNominatedAsync(nominatedEvent, cancelToken);
            await _movieCommandRepository.UpdateMovieAsync(movieId, movie.PosterUrl, cancelToken);
        }
        else
        {
            var nominatedEvent = new NominatedEventRecord(embeddedMovie, "nominated-event-" + movie.id, _dateTimeProvider.Now, user.Tenant.Id, user.Id);
            await _movieCommandRepository.InsertMovieAsync(movie, cancelToken);
            await _movieCommandRepository.InsertNominatedAsync(nominatedEvent, cancelToken);
        }

        var addMovieEvent = new AddMovieEvent(votingSessionId, embeddedMovie, user, movie.CreationYear.ToDecade());
        await _bus.Publish(addMovieEvent, cancelToken);

        var dto = new AknowledgedNominationDTO { Decade = movie.CreationYear.ToDecade().ToString()[1..], Message = "OK" };
        return dto.AsMaybe();
    }
}