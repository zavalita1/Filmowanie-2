using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Mappers;

internal sealed class NominationsMapper : INominationsMapper
{
    private readonly ILogger<NominationsMapper> _log;
    private readonly IMovieQueryRepository _movieQueryRepository;

    public NominationsMapper(ILogger<NominationsMapper> log, IMovieQueryRepository movieQueryRepository)
    {
        _log = log;
        _movieQueryRepository = movieQueryRepository;
    }

    public OperationResult<NominationsDataDTO> Map(OperationResult<(CurrentNominationsData, DomainUser)> maybe) => maybe.Accept(Map, _log);

    public OperationResult<NominationsDataDTO> Map((CurrentNominationsData, DomainUser) input)
    {
        var user = input.Item2;
        var nominationDecades = input.Item1.NominationData.Where(x => x.User.Id == user.Id).Select(x => x.Year.ToString()[1..]).ToArray();

        var result = new NominationsDataDTO { Nominations = nominationDecades };

        return new OperationResult<NominationsDataDTO>(result, null);
    }

    public Task<OperationResult<NominationsFullDataDTO>> EnrichNominationsAsync(OperationResult<(NominationsDataDTO, DomainUser)> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(EnrichNominationsAsync, _log, cancellationToken);

    // TODO add cron job that cleans old events
    public async Task<OperationResult<NominationsFullDataDTO>> EnrichNominationsAsync((NominationsDataDTO, DomainUser) input, CancellationToken cancellationToken)
    {
        var user = input.Item2;

        var movieCanBeNominatedAgainEvents = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(x => true, user.Tenant, cancellationToken);
        var movieNominatedAgainEvents = await _movieQueryRepository.GetMoviesNominatedAgainEventsAsync(x => true, user.Tenant, cancellationToken);

        var userNominationsDecades = input.Item1.Nominations.Select(StringExtensions.ToDecade).ToArray();
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

        var moviesThatCanBeNominatedAgain = await _movieQueryRepository.GetMoviesAsync(x => moviesThatCanBeNominatedAgainIds.Contains(x.id), user.Tenant, cancellationToken);

        var moviesThatCanBeNominatedAgainDTOs = moviesThatCanBeNominatedAgain
            .OrderBy(x => x.CreationYear)
            .Select(x => new MovieDTO(x.id, x.Name, x.PosterUrl, x.Description, x.FilmwebUrl, x.CreationYear, StringHelper.GetDurationString(x.DurationInMinutes), x.Genres, x.Actors, x.Directors, x.Writers, x.OriginalTitle))
            .ToArray();

        var result = new NominationsFullDataDTO { Nominations = input.Item1.Nominations, MoviesThatCanBeNominatedAgain = moviesThatCanBeNominatedAgainDTOs };
        return result.ToOperationResult();
    }
}