using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Repositories;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal interface IGetMoviesForVotingSessionVisitor : IOperationAsyncVisitor<(VotingSessionId, DomainUser), MovieDTO[]>;
internal interface IEnrichMoviesForVotingSessionWithPlaceholdersVisitor : IOperationAsyncVisitor<MovieDTO[], MovieDTO[]>;

internal sealed class MoviesVisitor : IGetMoviesForVotingSessionVisitor, IEnrichMoviesForVotingSessionWithPlaceholdersVisitor
{
    private readonly IRequestClient<MoviesListRequested> _requestClient;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly ILogger<MoviesVisitor> _log;

    public MoviesVisitor(IRequestClient<MoviesListRequested> requestClient, IMovieQueryRepository movieQueryRepository, ILogger<MoviesVisitor> log)
    {
        _requestClient = requestClient;
        _movieQueryRepository = movieQueryRepository;
        _log = log;
    }

    public async Task<OperationResult<MovieDTO[]>> VisitAsync(OperationResult<MovieDTO[]> movies, CancellationToken cancellationToken)
    {
        // TODO
        return movies with { Error = null };
    }

    public async Task<OperationResult<MovieDTO[]>> VisitAsync(OperationResult<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken)
    {
        var correlationId = input.Result.Item1.CorrelationId;
        var embeddedMovies = await _requestClient.GetResponse<CurrentVotingListResponse>(new MoviesListRequested(correlationId), cancellationToken);
        var moviesIds = embeddedMovies.Message.Movies.Select(x => x.Movie.id).ToArray();
        var moviesEntities = await _movieQueryRepository.GetMoviesAsync(x => moviesIds.Contains(x.id), cancellationToken);

        if (moviesEntities.Length != moviesIds.Length)
            return new OperationResult<MovieDTO[]>(null, new Error("Movies missing in DB!", ErrorType.InvalidState));

        var movies = embeddedMovies.Message.Movies.Join(moviesEntities, x => x.Movie.id, x => x.id, (x, y) => new { Movie = y, x.Votes });

        var resultMovies = new List<MovieDTO>(embeddedMovies.Message.Movies.Length);
        
        foreach (var movie in movies)
        {
            _log.LogInformation($"Mapping movie: {movie.Movie.Name}");
            var votes = (int?)movie.Votes.SingleOrDefault(x => x.User.id == input.Result.Item2.Id)?.VoteType ?? 0;
            var duration = GetDurationString(movie.Movie.DurationInMinutes);
            var movieDto = new MovieDTO(movie.Movie.id, movie.Movie.Name, votes, movie.Movie.PosterUrl, movie.Movie.Description, movie.Movie.FilmwebUrl, movie.Movie.CreationYear, duration, movie.Movie.Genres, movie.Movie.Actors,
                movie.Movie.Directors, movie.Movie.Writers, movie.Movie.OriginalTitle);

            resultMovies.Add(movieDto);
        }

        return new OperationResult<MovieDTO[]>(resultMovies.ToArray(), null);
    }

    private static string GetDurationString(int durationInMinutes)
    {
        var duration = TimeSpan.FromMinutes(durationInMinutes);
        var durationHours = (int)duration.TotalHours;
        var durationMinutes = (int)duration.TotalMinutes - 60 * durationHours;
        var result = durationMinutes == 0 ? $"{durationHours} godz." : $"{durationHours} godz. {durationMinutes} min.";
        return result;
    }
}