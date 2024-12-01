using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IVotingSessionCommandRepository
{
    public Task InsertAsync(IReadonlyVotingResult votingResult, CancellationToken cancellationToken);

    public Task UpdateAsync(string id, IEnumerable<IResultEmbeddedMovie> movies, IEnumerable<IReadOnlyEmbeddedUserWithNominationAward> usersAwards,
        DateTime concluded,
        IEnumerable<IReadOnlyEmbeddedMovieWithNominationContext> moviesAdded,
        IReadOnlyEmbeddedMovie winner,
        CancellationToken cancellationToken);
}