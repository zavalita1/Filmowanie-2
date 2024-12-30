using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IVotingSessionCommandRepository
{
    public Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancellationToken);

    public Task UpdateAsync(string id, IEnumerable<IReadOnlyEmbeddedMovieWithVotes> movies, IEnumerable<IReadOnlyEmbeddedUserWithNominationAward> usersAwards,
        DateTime concluded,
        IEnumerable<IReadOnlyEmbeddedMovieWithNominationContext> moviesAdded,
        IReadOnlyEmbeddedMovie winner,
        CancellationToken cancellationToken);
}