using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

internal sealed class MoviesForVotingSessionEnricher : IMoviesForVotingSessionEnricher
{
    private readonly IRequestClient<NominationsRequestedEvent> _getNominationsRequestClient;
    private readonly ILogger<MoviesForVotingSessionEnricher> _log;

    public MoviesForVotingSessionEnricher(IRequestClient<NominationsRequestedEvent> getNominationsRequestClient, ILogger<MoviesForVotingSessionEnricher> log)
    {
        _getNominationsRequestClient = getNominationsRequestClient;
        _log = log;
    }

    public Task<Maybe<MovieDTO[]>> EnrichWithPlaceholdersAsync(Maybe<(MovieDTO[], VotingSessionId)> movies, CancellationToken cancelToken) => movies.AcceptAsync(EnrichWithPlaceholders, _log, cancelToken);

    public async Task<Maybe<MovieDTO[]>> EnrichWithPlaceholders((MovieDTO[], VotingSessionId) movies, CancellationToken cancelToken)
    {
        var nominationsRequested = new NominationsRequestedEvent(movies.Item2);
        var nominations = await _getNominationsRequestClient.GetResponse<CurrentNominationsResponse>(nominationsRequested, cancelToken);

        var placeHolders = nominations.Message.Nominations.Where(x => x.Concluded == null).Select(GetPlaceholderDTO);
        var result = movies.Item1.Concat(placeHolders).ToArray();

        return new Maybe<MovieDTO[]>(result, null);
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
            Decade._2000s => "2k \ud83d\udcf1 \ud83d\udcbb \u2708\ufe0f \ud83c\udfe2\ud83c\udfe2 \ud83d\ude31",
            Decade._2010s => "2k-dziesiątych \ud83d\udc4d \ud83d\ude02 \ud83c\udf0e",
            Decade._2020s => "2k-dwudziestych \ud83e\udda0 \ud83d\ude37 \ud83d\udca5 \ud83e\udd1c",
            _ => ""
        };

        var placeholderTitle = $"{nominationData.User!.DisplayName} wybierze tutaj film z lat: {decadeTranslation}.";
        return new MovieDTO(placeholderTitle, (int)nominationData.Year);
    }
}