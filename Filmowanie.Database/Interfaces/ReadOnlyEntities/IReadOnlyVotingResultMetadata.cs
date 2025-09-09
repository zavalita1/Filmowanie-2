using Filmowanie.Abstractions;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyVotingResultMetadata
{
    DateTime Concluded { get; }
    MovieId WinnerMovieId { get; }
    VotingSessionId VotingResultId { get; }
    
    DomainUser WinnerNominatedBy { get; }
}