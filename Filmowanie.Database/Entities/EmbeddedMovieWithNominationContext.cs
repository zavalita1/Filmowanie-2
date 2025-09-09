using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovieWithNominatedBy : IReadOnlyEmbeddedMovieWithNominatedBy
{
    public virtual EmbeddedMovie Movie { get; set; }

    public virtual EmbeddedUser NominatedBy { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyEmbeddedMovieWithNominatedBy.NominatedBy => NominatedBy;
    IReadOnlyEmbeddedMovie IReadOnlyEmbeddedMovieWithNominatedBy.Movie => Movie;

    protected EmbeddedMovieWithNominatedBy()
    { }

public EmbeddedMovieWithNominatedBy(IReadOnlyEmbeddedMovieWithNominatedBy other)
    {
        Movie = other.Movie.AsMutable();
        NominatedBy = other.NominatedBy.AsMutable();
    }

}

public class EmbeddedMovieWithNominationContext : EmbeddedMovieWithNominatedBy, IReadOnlyEmbeddedMovieWithNominationContext
{
    public DateTime NominationConcluded { get; set; }
    public DateTime NominationStarted { get; set; }


    public EmbeddedMovieWithNominationContext() { }

    public EmbeddedMovieWithNominationContext(IReadOnlyEmbeddedMovieWithNominationContext other)
    {
        Movie = other.Movie.AsMutable();
        NominatedBy = other.NominatedBy.AsMutable();
        NominationConcluded = other.NominationConcluded;
        NominationStarted = other.NominationStarted;
    }

    public EmbeddedMovieWithNominationContext(IReadOnlyEmbeddedMovie other)
    {
        Movie = other.AsMutable();
    }
}