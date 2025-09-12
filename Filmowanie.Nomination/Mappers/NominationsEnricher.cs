using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Repositories;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Mappers;

internal sealed class NominationsEnricher : INominationsEnricher
{
    private readonly ILogger<NominationsEnricher> _log;
    private readonly IMovieDomainRepository _movieQueryRepository;

    public NominationsEnricher(ILogger<NominationsEnricher> log, IMovieDomainRepository movieQueryRepository)
    {
        _log = log;
        _movieQueryRepository = movieQueryRepository;
    }

    public Task<Maybe<NominationsFullDataDTO>> EnrichNominationsAsync(Maybe<NominationsDataDTO> input, CancellationToken cancelToken) =>
        input.AcceptAsync(EnrichNominationsAsync, _log, cancelToken);

    // TODO add cron job that cleans old events
    public async Task<Maybe<NominationsFullDataDTO>> EnrichNominationsAsync(NominationsDataDTO input, CancellationToken cancelToken)
    {
        var movieCanBeNominatedAgainEvents = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(cancelToken);
        var movieNominatedAgainEvents = await _movieQueryRepository.GetMovieNominatedEventsAsync(cancelToken);

        var userNominationsDecades = input.Nominations.Select(StringExtensions.ToDecade).ToArray();
        var filteredMoviesThatCanBeNominatedAgainEvents = movieCanBeNominatedAgainEvents
            .Where(x => userNominationsDecades.Contains(x.Movie.MovieCreationYear.ToDecade()))
            .GroupBy(x => x.Movie.id, x => x.Created)
            .ToDictionary(x => x.Key, x => x.Max());

        var filteredMovieNominatedAgainEvents = movieNominatedAgainEvents
            .Where(x => userNominationsDecades.Contains(x.Movie.MovieCreationYear.ToDecade()))
            .GroupBy(x => x.Movie.id, x => x.Created)
            .ToDictionary(x => x.Key, x => x.Max());

        var moviesThatCanBeNominatedAgainIds = filteredMoviesThatCanBeNominatedAgainEvents
            .Where(x => !filteredMovieNominatedAgainEvents.ContainsKey(x.Key) || filteredMovieNominatedAgainEvents[x.Key] < x.Value)
            .Select(x => x.Key);

        var moviesThatCanBeNominatedAgain = await _movieQueryRepository.GetManyByIdAsync(moviesThatCanBeNominatedAgainIds, cancelToken, false);

        var moviesThatCanBeNominatedAgainDTOs = moviesThatCanBeNominatedAgain.Result
            .OrderBy(x => x.CreationYear)
            .Select(x => new MovieDTO(x.id, x.Name, x.PosterUrl, x.BigPosterUrl, x.Description, x.FilmwebUrl, x.CreationYear, StringHelper.GetDurationString(x.DurationInMinutes), x.Genres, x.Actors, x.Directors, x.Writers, x.OriginalTitle))
            .ToArray();

        var result = new NominationsFullDataDTO { Nominations = input.Nominations, MoviesThatCanBeNominatedAgain = moviesThatCanBeNominatedAgainDTOs };
        return result.AsMaybe();
    }
}