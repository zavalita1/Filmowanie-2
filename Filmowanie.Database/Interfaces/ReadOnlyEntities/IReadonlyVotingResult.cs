namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyVotingResult : IReadOnlyEntity
{
    DateTime? Concluded { get; }

    IReadOnlyEmbeddedMovieWithVotes[] Movies { get; }
    IReadOnlyEmbeddedMovieWithVotes[]? ExtraVotingMovies { get; }
    IReadOnlyEmbeddedUserWithNominationAward[] UsersAwardedWithNominations { get; }

    IReadOnlyEmbeddedMovie[] MoviesGoingByeBye { get; }

    IReadOnlyEmbeddedMovieWithNominationContext[] MoviesAdded { get; }

    IReadOnlyEmbeddedMovieWithNominatedBy? Winner { get; }
}