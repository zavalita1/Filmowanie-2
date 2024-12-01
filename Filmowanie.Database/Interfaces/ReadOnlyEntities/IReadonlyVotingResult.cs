using Filmowanie.Database.Entities;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadonlyVotingResult : IReadOnlyEntity
{
    public DateTime? Concluded { get; }

    public IReadOnlyEmbeddedMovieWithVotes[] Movies { get; }
    public IReadOnlyEmbeddedUserWithNominationAward[] UsersAwardedWithNominations { get; }

    public IReadOnlyEmbeddedMovie[] MoviesGoingByeBye { get; }

    public IReadOnlyEmbeddedMovieWithNominationContext[] MoviesAdded { get; }

    IReadOnlyEmbeddedMovie Winner { get; }
}