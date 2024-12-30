using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovieWithNominationContext : IReadOnlyEmbeddedMovieWithNominationContext
{
    public EmbeddedMovie Movie { get; set; }

    public EmbeddedUser NominatedBy { get; set; }
    public DateTime NominationConcluded { get; set; }
    public DateTime NominationStarted { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyEmbeddedMovieWithNominationContext.NominatedBy => NominatedBy;
    IReadOnlyEmbeddedMovie IReadOnlyEmbeddedMovieWithNominationContext.Movie => Movie;


    public EmbeddedMovieWithNominationContext() { }

    public EmbeddedMovieWithNominationContext(IReadOnlyEmbeddedMovieWithNominationContext other)
    {
        Movie = other.Movie.AsMutable();
        NominatedBy = other.NominatedBy.AsMutable();
        NominationConcluded = other.NominationConcluded;
        NominationStarted = other.NominationStarted;
    }
}