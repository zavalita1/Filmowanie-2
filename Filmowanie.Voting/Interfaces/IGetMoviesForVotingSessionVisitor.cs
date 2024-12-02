using Filmowanie.Abstractions;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IGetMoviesForVotingSessionVisitor : IOperationAsyncVisitor<(VotingSessionId, DomainUser), MovieDTO[]>;