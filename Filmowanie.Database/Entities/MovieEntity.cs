using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class MovieEntity : Entity, IReadOnlyMovieEntity
{
    public virtual string Name { get; set; } = null!;

    public virtual string OriginalTitle { get; set; } = null!;

    public virtual string Description { get; set; } = null!;

    public virtual string PosterUrl { get; set; } = null!;
    public virtual string AltDescription { get; set; } = null!;
    public virtual string BigPosterUrl { get; set; } = null!;

    public virtual string FilmwebUrl { get; set; } = null!;
    public virtual string[] Actors { get; set; } = null!;
    public virtual string[] Writers { get; set; } = null!;
    public virtual string[] Directors { get; set; } = null!;
    public virtual string[] Genres { get; set; } = null!;
    public virtual int CreationYear { get; set; } 

    public virtual int DurationInMinutes { get; set; }
    public virtual bool? IsRejected { get; set; } = null!;

    public MovieEntity()
    {
    }

    public MovieEntity(IReadOnlyMovieEntity other)
    {
        Name = other.Name;
        OriginalTitle = other.OriginalTitle;
        Description = other.Description;
        PosterUrl = other.PosterUrl;
        BigPosterUrl = other.BigPosterUrl;
        AltDescription = other.AltDescription;
        FilmwebUrl = other.FilmwebUrl;
        Actors = other.Actors;
        Writers = other.Writers;
        Directors = other.Directors;
        Genres = other.Genres;
        CreationYear = other.CreationYear;
        DurationInMinutes = other.DurationInMinutes;
        id = other.id;
        Created = other.Created;
        TenantId = other.TenantId;
        IsRejected = other.IsRejected;
    }
}