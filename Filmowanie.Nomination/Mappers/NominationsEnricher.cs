using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Mappers;

internal sealed class NominationsEnricher : INominationsEnricher
{
    private readonly ILogger<NominationsEnricher> log;
    private readonly IMovieDomainRepository movieQueryRepository;

    public NominationsEnricher(ILogger<NominationsEnricher> log, IMovieDomainRepository movieQueryRepository)
    {
        this.log = log;
        this.movieQueryRepository = movieQueryRepository;
    }

    public Task<Maybe<NominationsFullDataDTO>> EnrichNominationsAsync(Maybe<NominationsDataDTO> input, CancellationToken cancelToken) =>
        input.AcceptAsync(EnrichNominationsAsync, this.log, cancelToken);

    // TODO add cron job that cleans old events
    public async Task<Maybe<NominationsFullDataDTO>> EnrichNominationsAsync(NominationsDataDTO input, CancellationToken cancelToken)
    {
        var movieCanBeNominatedAgainEvents = await this.movieQueryRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(cancelToken);
        var movieNominatedAgainEvents = await this.movieQueryRepository.GetMovieNominatedEventsAsync(cancelToken);

        var userNominationsDecades = input.Nominations.Select(StringExtensions.ToDecade).ToArray();
        var filteredMoviesThatCanBeNominatedAgainEvents = movieCanBeNominatedAgainEvents
            .Where(x => userNominationsDecades.Contains(x.Movie.MovieCreationYear.ToDecade()))
            .GroupBy(x => x.Movie.id, x => x.Created)
            .ToDictionary(x => x.Key, x => x.Max());

        var filteredMovieNominatedAgainEvents = movieNominatedAgainEvents
            .Where(x => userNominationsDecades.Contains(x.MovieCreationYear.ToDecade()))
            .GroupBy(x => x.MovieId, x => x.Created)
            .ToDictionary(x => x.Key, x => x.Max());

        var moviesThatCanBeNominatedAgainIds = filteredMoviesThatCanBeNominatedAgainEvents
            .Where(x => !filteredMovieNominatedAgainEvents.ContainsKey(x.Key) || filteredMovieNominatedAgainEvents[x.Key] < x.Value)
            .Select(x => x.Key);

        var moviesThatCanBeNominatedAgain = await this.movieQueryRepository.GetManyByIdAsync(moviesThatCanBeNominatedAgainIds, cancelToken, false);

        if (moviesThatCanBeNominatedAgain.Error.HasValue)
            return moviesThatCanBeNominatedAgain.Error.Value.ChangeResultType<IReadOnlyMovieEntity[], NominationsFullDataDTO>();

        var moviesThatCanBeNominatedAgainDtos = moviesThatCanBeNominatedAgain.Result!
            .Where(x => x.IsRejected != true)
            .OrderBy(x => x.CreationYear)
            .Select(x => new MovieDTO(x.id, x.Name, x.PosterUrl, x.BigPosterUrl, x.Description, x.FilmwebUrl, x.CreationYear, x.DurationInMinutes.GetDurationString(), x.Genres, x.Actors, x.Directors, x.Writers, x.OriginalTitle))
            .ToArray();

        var result = new NominationsFullDataDTO { Nominations = input.Nominations, MoviesThatCanBeNominatedAgain = moviesThatCanBeNominatedAgainDtos };
        return result.AsMaybe();
    }
}