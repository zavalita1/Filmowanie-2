using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Mappers;

internal sealed class VotingSessionMapper : IVotingSessionMapper
{
    private readonly ILogger<VotingSessionMapper> _log;

    public VotingSessionMapper(ILogger<VotingSessionMapper> log)
    {
        _log = log;
    }

    public Maybe<VotingState> Map(Maybe<VotingSessionId?> input) => input.Accept(Map, _log);

    public Maybe<VotingSessionId?> Map(Maybe<string> input) => input.Accept(Visit, _log);

    public Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input) => input.Accept(Map, _log);

    public Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input) => input.Accept(Map, _log);

    private Maybe<HistoryDTO> Map(WinnerMetadata[] input)
    {
        var entries = input
            .Select(x => new HistoryEntryDTO(x.Name, x.OriginalTitle, x.CreationYear, x.NominatedBy, x.Watched.ToString("d", new CultureInfo("pl"))))
            .ToArray();

        var result = new HistoryDTO(entries);

        return result.AsMaybe();
    }

    private static Maybe<VotingSessionsDTO> Map(VotingMetadata[] input)
    {
        var dto = input
            .OrderByDescending(x => x.Concluded)
            .Select(x => new VotingSessionDTO(x.VotingSessionId, x.Concluded.ToString("D", new CultureInfo("pl")), x.Concluded.ToString("s")))
            .ToArray();
        var result = new VotingSessionsDTO(dto);

        return result.AsMaybe();
    }

    private static Maybe<VotingState> Map(VotingSessionId? input)
    {
        var state = input == null ? VotingState.Results : VotingState.Voting;
        return state.AsMaybe();
    }

    private static Maybe<VotingSessionId?> Visit(string input)
    {
        if (string.IsNullOrEmpty(input))
            return ((VotingSessionId?)null).AsMaybe();

        if (!Guid.TryParse(input, out var correlationId))
            return new Error("Invalid id!", ErrorType.IncomingDataIssue).AsMaybe<VotingSessionId?>();

        return (new VotingSessionId(correlationId) as VotingSessionId?).AsMaybe();
    }
}