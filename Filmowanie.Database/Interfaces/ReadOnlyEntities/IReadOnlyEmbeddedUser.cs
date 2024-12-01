using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;

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

public interface IReadOnlyEmbeddedMovieWithNominationContext : IReadOnlyEmbeddedMovie
{
    public IReadOnlyEmbeddedUser NominatedBy { get; }

    public DateTime NominationConcluded { get; }
    public DateTime NominationStarted { get; }
}

public interface IReadOnlyEmbeddedMovieWithVotes : IReadOnlyEmbeddedMovie
{
    public IEnumerable<IReadOnlyVote> Votes { get; }
}

public interface IResultEmbeddedMovie
{
    public IReadOnlyEmbeddedMovieWithVotes Movie { get; }
    public int VotingScore { get; }
}