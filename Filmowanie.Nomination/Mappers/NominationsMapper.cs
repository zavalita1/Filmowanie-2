using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Mappers;

internal sealed class NominationsMapper : INominationsMapper
{
    private readonly ILogger<NominationsEnricher> log;

    public NominationsMapper(ILogger<NominationsEnricher> log)
    {
        this.log = log;
    }

    public Maybe<NominationsDataDTO> Map(Maybe<CurrentNominationsData> maybeCurrentNominationsData, Maybe<DomainUser> maybeUser) => maybeCurrentNominationsData.Merge(maybeUser).Accept(Map, this.log);

    private static Maybe<NominationsDataDTO> Map((CurrentNominationsData, DomainUser) input)
    {
        var user = input.Item2;
        var nominationDecades = input.Item1.NominationData.Where(x => x!.User!.Id == user.Id && x.Concluded == null).Select(x => x.Year.ToString()[1..]).ToArray();

        var result = new NominationsDataDTO { Nominations = nominationDecades };

        return new Maybe<NominationsDataDTO>(result, null);
    }
}