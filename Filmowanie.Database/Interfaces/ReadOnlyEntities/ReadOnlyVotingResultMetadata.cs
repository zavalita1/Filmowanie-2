using Filmowanie.Abstractions;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public readonly record struct ReadOnlyVotingResultMetadata(DateTime Concluded, MovieId WinnerMovieId, VotingSessionId VotingResultId, DomainUser WinnerNominatedBy) : IReadOnlyVotingResultMetadata;

