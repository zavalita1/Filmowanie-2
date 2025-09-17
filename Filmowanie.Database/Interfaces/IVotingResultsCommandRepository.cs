using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IVotingResultsCommandRepository
{
    Task<Maybe<VoidResult>> ResetAsync(VotingSessionId id, CancellationToken cancelToken);

    Task<Maybe<VoidResult>> UpdateAsync(VotingSessionId id, IEnumerable<IReadOnlyEmbeddedMovieWithVotes> movies, IEnumerable<IReadOnlyEmbeddedUserWithNominationAward> usersAwards, DateTime concluded,
        IEnumerable<IReadOnlyEmbeddedMovieWithNominationContext> moviesAdded, IReadOnlyEmbeddedMovieWithNominatedBy winner, IEnumerable<IReadOnlyEmbeddedMovie> moviesToRemove, CancellationToken cancelToken);

    Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancelToken);
}