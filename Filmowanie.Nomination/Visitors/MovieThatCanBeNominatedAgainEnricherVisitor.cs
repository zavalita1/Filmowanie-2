using Filmowanie.Abstractions;
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

    public async Task<OperationResult<NominationsFullDataDTO>> VisitAsync(OperationResult<(NominationsDataDTO, DomainUser)> input, CancellationToken cancellationToken)
    {
        var user = input.Result.Item2;

        var moviesThatCanBeNominatedAgainList = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEntityAsync(x => x.TenantId == user.Tenant.Id, cancellationToken);
        var moviesThatCanBeNominatedAgainIds = moviesThatCanBeNominatedAgainList?.Movies.Select(x => x.id).ToArray() ?? [];
        var moviesThatCanBeNominatedAgain = await _movieQueryRepository.GetMoviesAsync(x => moviesThatCanBeNominatedAgainIds.Contains(x.id), cancellationToken);

        var moviesThatCanBeNominatedAgainDTOs = moviesThatCanBeNominatedAgain.Select(x => new MovieDTO(x.id, x.Name, x.PosterUrl, x.Description, x.FilmwebUrl, x.CreationYear,
            StringHelper.GetDurationString(x.DurationInMinutes), x.Genres, x.Actors, x.Directors, x.Writers, x.OriginalTitle)).ToArray();

        var result = new NominationsFullDataDTO { Nominations = input.Result.Item1.Nominations, MoviesThatCanBeNominatedAgain =  moviesThatCanBeNominatedAgainDTOs};
        return new OperationResult<NominationsFullDataDTO>(result, null);
    }

    public ILogger Log => _log;
}