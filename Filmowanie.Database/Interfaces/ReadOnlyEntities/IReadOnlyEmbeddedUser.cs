
namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyEmbeddedUser
{
   public string id { get; }
   public string Name { get; }

   public int TenantId { get; }
}

public interface IReadOnlyEmbeddedMovie
{
    public string id { get; }
    public string Name { get; }
    public int MovieCreationYear { get; }
}

public interface IReadOnlyEmbeddedMovieWithNominatedBy
{
    public IReadOnlyEmbeddedMovie Movie { get; }
    public IReadOnlyEmbeddedUser NominatedBy { get; }
}

public interface IReadOnlyEmbeddedMovieWithNominationContext : IReadOnlyEmbeddedMovieWithNominatedBy
{
    public DateTime NominationConcluded { get; }
    public DateTime NominationStarted { get; }
}

public interface IReadOnlyEmbeddedMovieWithVotes
{
    public IReadOnlyEmbeddedMovie Movie { get;  }

    public IEnumerable<IReadOnlyVote> Votes { get; }
    public int VotingScore { get; }
}
