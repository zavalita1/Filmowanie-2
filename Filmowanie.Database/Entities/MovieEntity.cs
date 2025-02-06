using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class MovieEntity : Entity, IReadOnlyMovieEntity
{
    public virtual string Name { get; set; }

    public virtual string OriginalTitle { get; set; }

    public virtual string Description { get; set; }

    public virtual string PosterUrl { get; set; }

    public virtual string FilmwebUrl { get; set; }
    public virtual string[] Actors { get; set; }
    public virtual string[] Writers { get; set; }
    public virtual string[] Directors { get; set; }
    public virtual string[] Genres { get; set; }
    public virtual int CreationYear { get; set; }

    public virtual int DurationInMinutes { get; set; }

    public MovieEntity()
    {
    }

    public MovieEntity(IReadOnlyMovieEntity other)
    {
        Name = other.Name;
        OriginalTitle = other.OriginalTitle;
        Description = other.Description;
        PosterUrl = other.PosterUrl;
        FilmwebUrl = other.FilmwebUrl;
        Actors = other.Actors;
        Writers = other.Writers;
        Directors = other.Directors;
        Genres = other.Genres;
        CreationYear = other.CreationYear;
        DurationInMinutes = other.DurationInMinutes;
        id = ((IReadOnlyEntity)other).id;
        Created = other.Created;
        TenantId = other.TenantId;
    }
}