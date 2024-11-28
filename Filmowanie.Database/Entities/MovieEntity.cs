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
}

internal class VotingResult : Entity, IReadonlyVotingResult
{
    public virtual DateTime? Concluded { get; set; }
}

public interface IReadonlyVotingResult : IReadOnlyEntity
{
    public DateTime? Concluded { get;  }
}