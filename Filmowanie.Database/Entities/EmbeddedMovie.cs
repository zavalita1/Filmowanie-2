using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovie : IReadOnlyEmbeddedMovie
{
    public virtual string id { get; set; }

    public virtual string Name { get; set; }

    public virtual int MovieCreationYear { get; set; }

    public EmbeddedMovie()
    { }

    public EmbeddedMovie(IReadOnlyEmbeddedMovie other)
    {
        id = other.id;
        Name = other.Name;
        MovieCreationYear = other.MovieCreationYear;
    }
}

public class EmbeddedMovieWithNominationContext : IReadOnlyEmbeddedMovieWithNominationContext
{
    public EmbeddedMovie Movie { get; set; }

    public EmbeddedUser NominatedBy { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyEmbeddedMovieWithNominationContext.NominatedBy => NominatedBy;
    IReadOnlyEmbeddedMovie IReadOnlyEmbeddedMovieWithNominationContext.Movie => Movie;

    public DateTime NominationConcluded { get; set; }
    public DateTime NominationStarted { get; set; }

    public int MovieCreationYear { get; set; }
}