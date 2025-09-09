using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting;

public class VotingResult : Entity, IReadOnlyVotingResult
{
    public IEnumerable<EmbeddedMovieWithVotes> Movies { get; set; } = new List<EmbeddedMovieWithVotes>();
    public IEnumerable<EmbeddedUserWithNominationAward> UsersAwardedWithNominations { get; set; } = new List<EmbeddedUserWithNominationAward>();
    public IEnumerable<EmbeddedMovie> MoviesGoingByeBye { get; set; } = new List<EmbeddedMovie>();
    public IEnumerable<EmbeddedMovieWithNominationContext> MoviesAdded { get; set; } = new List<EmbeddedMovieWithNominationContext>();
    public virtual DateTime? Concluded { get; set; }
    public virtual EmbeddedMovieWithNominatedBy Winner { get; set; }

    IReadOnlyEmbeddedMovieWithVotes[] IReadOnlyVotingResult.Movies => Movies.Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray();
    IReadOnlyEmbeddedUserWithNominationAward[] IReadOnlyVotingResult.UsersAwardedWithNominations => UsersAwardedWithNominations.Cast<IReadOnlyEmbeddedUserWithNominationAward>().ToArray();
    IReadOnlyEmbeddedMovie[] IReadOnlyVotingResult.MoviesGoingByeBye => MoviesGoingByeBye.Cast<IReadOnlyEmbeddedMovie>().ToArray();
    IReadOnlyEmbeddedMovieWithNominationContext[] IReadOnlyVotingResult.MoviesAdded => MoviesAdded.Cast<IReadOnlyEmbeddedMovieWithNominationContext>().ToArray();
    IReadOnlyEmbeddedMovieWithNominatedBy IReadOnlyVotingResult.Winner => Winner;

    public VotingResult()
    { }

    public VotingResult(IReadOnlyVotingResult other) : this()
    {
        Movies = other.Movies.Select(x => x.AsMutable());
        UsersAwardedWithNominations = other.UsersAwardedWithNominations.Select(x => x.AsMutable());
        MoviesGoingByeBye = other.MoviesGoingByeBye.Select(x => x.AsMutable());
        MoviesAdded = other.MoviesAdded.Select(x => x.AsMutable());
        Concluded = other.Concluded;
        Winner = other.Winner?.AsMutable();
        Created = other.Created;
        TenantId = other.TenantId;
        id = other.id;
    }
}