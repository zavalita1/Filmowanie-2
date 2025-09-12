using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public readonly record struct ReadOnlyVotingResultMetadata(DateTime Concluded, MovieId WinnerMovieId, VotingSessionId VotingResultId, DomainUser WinnerNominatedBy) : IReadOnlyVotingResultMetadata;

