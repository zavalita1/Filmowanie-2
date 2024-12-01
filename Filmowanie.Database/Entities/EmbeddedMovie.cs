using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovie : IReadOnlyEmbeddedMovie
{
    public virtual string id { get; set; }

    public virtual string Name { get; set; }

    public virtual int MovieCreationYear { get; set; }
}

public class EmbeddedMovieWithNominationContext : IReadOnlyEmbeddedMovieWithNominationContext
{
    public string id { get; set; }

    public string Name { get; set; }

    public EmbeddedUser NominatedBy { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyEmbeddedMovieWithNominationContext.NominatedBy => NominatedBy;

    public DateTime NominationConcluded { get; set; }
    public DateTime NominationStarted { get; set; }

    public int MovieCreationYear { get; set; }
}