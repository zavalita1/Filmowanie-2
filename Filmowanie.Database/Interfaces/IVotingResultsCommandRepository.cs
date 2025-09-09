using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IVotingResultsCommandRepository
{
    Task<Maybe<VoidResult>> UpdateAsync(string id, IEnumerable<IReadOnlyEmbeddedMovieWithVotes> movies, IEnumerable<IReadOnlyEmbeddedUserWithNominationAward> usersAwards, DateTime concluded,
        IEnumerable<IReadOnlyEmbeddedMovieWithNominationContext> moviesAdded, IReadOnlyEmbeddedMovieWithNominatedBy winner, CancellationToken cancelToken);

    Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancelToken);
}