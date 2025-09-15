using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovie : IReadOnlyEmbeddedMovie
{
    public virtual string id { get; set; } = null!;

    public virtual string Name { get; set; } = null!;

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