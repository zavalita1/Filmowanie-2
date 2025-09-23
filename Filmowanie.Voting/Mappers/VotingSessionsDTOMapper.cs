using System.Globalization;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Mappers;

// TODO UTs
internal sealed class VotingSessionsDTOMapper : IVotingSessionsDTOMapper
{
    private readonly ILogger<VotingSessionsDTOMapper> log;

    public VotingSessionsDTOMapper(ILogger<VotingSessionsDTOMapper> log)
    {
        this.log = log;
    }

    public Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input) => input.Accept(Map, this.log);

    private static Maybe<VotingSessionsDTO> Map(VotingMetadata[] input)
    {
        var dto = input
            .OrderByDescending(x => x.Concluded)
            .Select(x => new VotingSessionDTO(x.VotingSessionId, x.Concluded.ToString("D", new CultureInfo("pl")), x.Concluded.ToString("s")))
            .ToArray();
        var result = new VotingSessionsDTO(dto);

        return result.AsMaybe();
    }
}