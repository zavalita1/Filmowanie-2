using Filmowanie.Abstractions;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IEnrichMoviesForVotingSessionWithPlaceholdersVisitor : IOperationAsyncVisitor<(MovieDTO[], VotingSessionId), MovieDTO[]>;