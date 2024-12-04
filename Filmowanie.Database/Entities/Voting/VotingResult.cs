using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting;

public class VotingResult : Entity, IReadonlyVotingResult
{
    public IEnumerable<EmbeddedMovieWithVotes> Movies { get; set; } = new List<EmbeddedMovieWithVotes>();
    public IEnumerable<EmbeddedUserWithNominationAward> UsersAwardedWithNominations { get; set; } = new List<EmbeddedUserWithNominationAward>();
    public IEnumerable<EmbeddedMovie> MoviesGoingByeBye { get; set; } = new List<EmbeddedMovie>();
    public IEnumerable<EmbeddedMovieWithNominationContext> MoviesAdded { get; set; } = new List<EmbeddedMovieWithNominationContext>();
    public virtual DateTime? Concluded { get; set; }
    public virtual EmbeddedMovie Winner { get; set; }


    IReadOnlyEmbeddedMovieWithVotes[] IReadonlyVotingResult.Movies => Movies.Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray();
    IReadOnlyEmbeddedUserWithNominationAward[] IReadonlyVotingResult.UsersAwardedWithNominations => UsersAwardedWithNominations.Cast<IReadOnlyEmbeddedUserWithNominationAward>().ToArray();
    IReadOnlyEmbeddedMovie[] IReadonlyVotingResult.MoviesGoingByeBye => MoviesGoingByeBye.Cast<IReadOnlyEmbeddedMovie>().ToArray();
    IReadOnlyEmbeddedMovieWithNominationContext[] IReadonlyVotingResult.MoviesAdded => MoviesAdded.Cast<IReadOnlyEmbeddedMovieWithNominationContext>().ToArray();
    IReadOnlyEmbeddedMovie IReadonlyVotingResult.Winner => Winner;
}