using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Database.Interfaces;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Visitors;

internal sealed class MovieThatCanBeNominatedAgainEnricherVisitor : IMovieThatCanBeNominatedAgainEnricherVisitor
{
    private readonly ILogger<MovieThatCanBeNominatedAgainEnricherVisitor> _log;
    private readonly IMovieQueryRepository _movieQueryRepository;

    public MovieThatCanBeNominatedAgainEnricherVisitor(ILogger<MovieThatCanBeNominatedAgainEnricherVisitor> log, IMovieQueryRepository movieQueryRepository)
    {
        _log = log;
        _movieQueryRepository = movieQueryRepository;
    }

    // TODO add cron job that cleans old events
    public async Task<OperationResult<NominationsFullDataDTO>> VisitAsync(OperationResult<(NominationsDataDTO, DomainUser)> input, CancellationToken cancellationToken)
    {
        var user = input.Result.Item2;

        var moviesThatCanBeNominatedAgainList = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEntityAsync(x => x.TenantId == user.Tenant.Id, cancellationToken);
        var moviesNominatedAgainList = await _movieQueryRepository.GetMoviesNominatedAgainEntityAsync(x => x.TenantId == user.Tenant.Id, cancellationToken);

        var userNominationsDecades = input.Result.Item1.Nominations.Select(StringExtensions.ToDecade).ToArray();
        var filteredMoviesThatCanBeNominatedAgain = moviesThatCanBeNominatedAgainList
            .Where(x => userNominationsDecades.Contains(x.Movie.MovieCreationYear.ToDecade()))
            .GroupBy(x => x.Movie.id, x => x.Created)
            .ToDictionary(x => x.Key, x => x.Max());

        var filteredMoviesNominatedAgainList = moviesNominatedAgainList.Where(x => userNominationsDecades.Contains(x.Movie.MovieCreationYear.ToDecade()))
            .GroupBy(x => x.Movie.id, x => x.Created)
            .ToDictionary(x => x.Key, x => x.Max());

        var moviesThatCanBeNominatedAgainIds = filteredMoviesThatCanBeNominatedAgain
            .Where(x => !filteredMoviesNominatedAgainList.ContainsKey(x.Key) || filteredMoviesNominatedAgainList[x.Key] < x.Value)
            .Select(x => x.Key);

        var moviesThatCanBeNominatedAgain = await _movieQueryRepository.GetMoviesAsync(x => moviesThatCanBeNominatedAgainIds.Contains(x.id), cancellationToken);

        var moviesThatCanBeNominatedAgainDTOs = moviesThatCanBeNominatedAgain
            .OrderBy(x => x.CreationYear)
            .Select(x => new MovieDTO(x.id, x.Name, x.PosterUrl, x.Description, x.FilmwebUrl, x.CreationYear,
            StringHelper.GetDurationString(x.DurationInMinutes), x.Genres, x.Actors, x.Directors, x.Writers, x.OriginalTitle)).ToArray();

        var result = new NominationsFullDataDTO { Nominations = input.Result.Item1.Nominations, MoviesThatCanBeNominatedAgain =  moviesThatCanBeNominatedAgainDTOs};
        return new OperationResult<NominationsFullDataDTO>(result, null);
    }

    public ILogger Log => _log;
}