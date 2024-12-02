using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class MoviesVisitor : IGetMoviesForVotingSessionVisitor, IEnrichMoviesForVotingSessionWithPlaceholdersVisitor
{
    private readonly IRequestClient<MoviesListRequested> _getMoviesListRequestClient;
    private readonly IRequestClient<NominationsRequested> _getNominationsRequestClient;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly ILogger<MoviesVisitor> _log;

    public MoviesVisitor(IRequestClient<MoviesListRequested> getMoviesListRequestClient, IRequestClient<NominationsRequested> getNominationsRequestClient, IMovieQueryRepository movieQueryRepository, ILogger<MoviesVisitor> log)
    {
        _getMoviesListRequestClient = getMoviesListRequestClient;
        _getNominationsRequestClient = getNominationsRequestClient;
        _movieQueryRepository = movieQueryRepository;
        _log = log;
    }

    public async Task<OperationResult<MovieDTO[]>> VisitAsync(OperationResult<(MovieDTO[], VotingSessionId)> movies, CancellationToken cancellationToken)
    {
        var nominationsRequested = new NominationsRequested(movies.Result.Item2.CorrelationId);
        var nominations = await _getNominationsRequestClient.GetResponse<CurrentNominationsResponse>(nominationsRequested, cancellationToken);

        var placeHolders = nominations.Message.Nominations.Where(x => x.Concluded == null).Select(GetPlaceholderDTO);
        var result = movies.Result.Item1.Concat(placeHolders).ToArray();

        return new OperationResult<MovieDTO[]>(result, null);
    }

    public async Task<OperationResult<MovieDTO[]>> VisitAsync(OperationResult<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken)
    {
        var correlationId = input.Result.Item1.CorrelationId;
        var embeddedMovies = await _getMoviesListRequestClient.GetResponse<CurrentVotingListResponse>(new MoviesListRequested(correlationId), cancellationToken);
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
            var duration = StringHelper.GetDurationString(movie.Movie.DurationInMinutes);
            var movieDto = new MovieDTO(movie.Movie.id, movie.Movie.Name, votes, movie.Movie.PosterUrl, movie.Movie.Description, movie.Movie.FilmwebUrl, movie.Movie.CreationYear, duration, movie.Movie.Genres, movie.Movie.Actors,
                movie.Movie.Directors, movie.Movie.Writers, movie.Movie.OriginalTitle);

            resultMovies.Add(movieDto);
        }

        return new OperationResult<MovieDTO[]>(resultMovies.ToArray(), null);
    }

    private static MovieDTO GetPlaceholderDTO(NominationData nominationData)
    {
        var decadeTranslation = nominationData.Year switch
        {
            Decade._1940s => "czterdziestych \ud83d\udd2b",
            Decade._1950s => "pięćdziesiątych \ud83c\udf99\ufe0f \ud83d\udcfb",
            Decade._1960s => "sześćdziesiątych \u262e\ufe0f \ud83c\udf08 \ud83d\ude80 \ud83d\udc68\u200d\ud83d\ude80",
            Decade._1970s => "siedemdziesiątych \ud83c\udfb8 \ud83d\udcfa \ud83c\udfb8",
            Decade._1980s => "osiemdziesiątych  \ud83c\udfb8 \ud83d\udd7a \ud83d\udc7e \ud83c\udfae",
            Decade._1990s => "dziewięćdziesiątych \ud83d\udd7a \ud83d\udcbe \ud83d\udcdf \ud83d\udcb2",
            Decade._2000s => "dwutysięcznych \ud83d\udcf1 \ud83d\udcbb \u2708\ufe0f \ud83c\udfe2\ud83c\udfe2 \ud83d\ude31",
            Decade._2010s => "dwutysięcznych-dziesiątych \ud83d\udc4d \ud83d\ude02 \ud83c\udf0e",
            Decade._2020s => "dwutysięcznychdwudziestych \ud83e\udda0 \ud83d\ude37 \ud83d\udca5 \ud83e\udd1c",
            _ => ""
        };

        var placeholderTitle = $"{nominationData.User.DisplayName} wybierze tutaj film z lat: {decadeTranslation}.";
        return new MovieDTO(placeholderTitle, (int)nominationData.Year);
    }

    public ILogger Log => _log;
}