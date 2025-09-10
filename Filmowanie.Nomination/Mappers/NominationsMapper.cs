using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Repositories;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Mappers;

internal sealed class NominationsMapper : INominationsMapper
{
    private readonly ILogger<NominationsMapper> _log;
    private readonly IMovieDomainRepository _movieQueryRepository;

    public NominationsMapper(ILogger<NominationsMapper> log, IMovieDomainRepository movieQueryRepository)
    {
        _log = log;
        _movieQueryRepository = movieQueryRepository;
    }

    public Maybe<NominationsDataDTO> Map(Maybe<(CurrentNominationsData, DomainUser)> maybe) => maybe.Accept(Map, _log);

    public Maybe<NominationsDataDTO> Map((CurrentNominationsData, DomainUser) input)
    {
        var user = input.Item2;
        var nominationDecades = input.Item1.NominationData.Where(x => x.User.Id == user.Id && x.Concluded == null).Select(x => x.Year.ToString()[1..]).ToArray();

        var result = new NominationsDataDTO { Nominations = nominationDecades };

        return new Maybe<NominationsDataDTO>(result, null);
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
            .Select(x => new MovieDTO(x.id, x.Name, x.PosterUrl, x.Description, x.FilmwebUrl, x.CreationYear, StringHelper.GetDurationString(x.DurationInMinutes), x.Genres, x.Actors, x.Directors, x.Writers, x.OriginalTitle))
            .ToArray();

        var result = new NominationsFullDataDTO { Nominations = input.Nominations, MoviesThatCanBeNominatedAgain = moviesThatCanBeNominatedAgainDTOs };
        return result.AsMaybe();
    }
}