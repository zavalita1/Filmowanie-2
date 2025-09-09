using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IVotingResultsRepository
{
    Task<Maybe<IReadOnlyVotingResult?>> GetByIdAsync(VotingSessionId id, CancellationToken cancelToken);
    Task<Maybe<IReadOnlyVotingResult?>> GetUnconcludedResultAsync(CancellationToken cancelToken);
    Task<Maybe<IEnumerable<IReadOnlyVotingResult>>> GetLastNVotingResultsAsync(int numberOfResultsToReturn, CancellationToken cancelToken);

    Task<Maybe<IEnumerable<IReadOnlyVotingResultMetadata>>> GetAllVotingResultsMetadataAsync(CancellationToken cancelToken);
}