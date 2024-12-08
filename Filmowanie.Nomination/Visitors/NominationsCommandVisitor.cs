using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Visitors;

internal sealed class NominationsCommandVisitor : INominationsResetterVisitor, INominationsCompleterVisitor
{
    private readonly ILogger<INominationsResetterVisitor> _log;
    private readonly IFilmwebPathResolver _filmwebPathResolver;
    private readonly IFilmwebHandler _filmwebHandler;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly IBus _bus;
    private readonly IDateTimeProvider _dateTimeProvider;

    public NominationsCommandVisitor(ILogger<INominationsResetterVisitor> log, IFilmwebPathResolver filmwebPathResolver, IFilmwebHandler filmwebHandler, IMovieCommandRepository movieCommandRepository, IMovieQueryRepository movieQueryRepository, IBus bus, IDateTimeProvider dateTimeProvider)
    {
        _log = log;
        _filmwebPathResolver = filmwebPathResolver;
        _filmwebHandler = filmwebHandler;
        _movieCommandRepository = movieCommandRepository;
        _movieQueryRepository = movieQueryRepository;
        _bus = bus;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<OperationResult<AknowledgedNominationDTO>> VisitAsync(OperationResult<(NominationDTO Dto, DomainUser User, CurrentNominationsResponse CurrentNominations)> input, CancellationToken cancellationToken)
    {
        var metadata = _filmwebPathResolver.GetMetadata(input.Result.Dto.MovieFilmwebUrl);
        var user = input.Result.User;
        var movie = await _filmwebHandler.GetMovie(metadata, user.Tenant, input.Result.Dto.PosterUrl, cancellationToken);
        var embeddedMovie = new EmbeddedMovie { id = movie.id, MovieCreationYear = movie.CreationYear, Name = movie.Name };

        if (movie.Genres.Contains("horror", StringComparer.OrdinalIgnoreCase))
            return new OperationResult<AknowledgedNominationDTO>(null, new Error("Horrors are not allowed. Nice try motherfucker.", ErrorType.IncomingDataIssue));

        var allowedDecades = input.Result.CurrentNominations.Nominations.Where(x => x.User.Id == user.Id).Select(x => x.Year).ToArray();
        if (allowedDecades.All(x => x != movie.CreationYear.ToDecade()))
            return new OperationResult<AknowledgedNominationDTO>(null, new Error($"This movie is not from decades: {string.Join(',', allowedDecades)}!", ErrorType.IncomingDataIssue));

        var existingMovies = await _movieQueryRepository.GetMoviesAsync(x => x.TenantId == user.Tenant.Id && x.Name == movie.Name, cancellationToken);
        if (existingMovies.Any())
        {
            var canBeNominatedAgainEvents = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEntityAsync(x => x.TenantId == user.Tenant.Id && x.Movie.Name == movie.Name, cancellationToken);
            var nominatedAgainEvents = await _movieQueryRepository.GetMoviesNominatedAgainEntityAsync(x => x.TenantId == user.Tenant.Id && x.Movie.Name == movie.Name, cancellationToken);

            var maxDateCanBeNominatedAgain = canBeNominatedAgainEvents.OrderByDescending(x => x.Created).FirstOrDefault()?.Created;
            var maxDateNominatedAgain = nominatedAgainEvents.OrderByDescending(x => x.Created).FirstOrDefault()?.Created ?? DateTime.MinValue;
            var canBeNominatedAgain = maxDateCanBeNominatedAgain > maxDateNominatedAgain;

            if (!canBeNominatedAgain)
                return new OperationResult<AknowledgedNominationDTO>(null, new Error("This movie had already been watched or is still waiting until it can be nominated again!", ErrorType.IncomingDataIssue));

            var movieId = canBeNominatedAgainEvents[0].Movie.id;
            embeddedMovie = new EmbeddedMovie { id = movieId, MovieCreationYear = movie.CreationYear, Name = movie.Name };
            var nominatedAgainEvent = new NominatedMovieAgainEventRecord(embeddedMovie, movieId, _dateTimeProvider.Now, user.Tenant.Id);
            await _movieCommandRepository.InsertNominatedAgainAsync(nominatedAgainEvent, cancellationToken);
        }
        else
        {
            await _movieCommandRepository.InsertMovieAsync(movie, cancellationToken);
        }

        var addMovieEvent = new AddMovieEvent(input.Result.CurrentNominations.CorrelationId, embeddedMovie, user, movie.CreationYear.ToDecade());
        await _bus.Publish(addMovieEvent, cancellationToken);

        var dto = new AknowledgedNominationDTO { Decade = movie.CreationYear.ToDecade().ToString()[1..], Message = "OK"};
        return new OperationResult<AknowledgedNominationDTO>(dto, null);
    }

    public async Task<OperationResult<AknowledgedNominationDTO>> VisitAsync(OperationResult<(string MovieId, DomainUser User, VotingSessionId VotingSessionId)> input, CancellationToken cancellationToken)
    {
        var (movieId, user, votingSessionId) = input.Result;
        var movies = await _movieQueryRepository.GetMoviesAsync(x => x.id == movieId, cancellationToken);

        if (!movies.Any())
            return new OperationResult<AknowledgedNominationDTO>(null, new Error("No such movie found!", ErrorType.IncomingDataIssue));

        var movieEntity = movies.Single();
        var movie = new EmbeddedMovie { id = movieEntity.id, MovieCreationYear = movieEntity.CreationYear, Name = movieEntity.Name };

        await _bus.Publish(new RemoveMovieEvent(votingSessionId.CorrelationId, movie , user), cancellationToken);
        return new OperationResult<AknowledgedNominationDTO>(new AknowledgedNominationDTO { Message = "OK" }, null);
    }

    public ILogger Log => _log;
}