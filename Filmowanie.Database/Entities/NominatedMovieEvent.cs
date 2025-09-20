using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class NominatedMovieEvent : Event, IReadOnlyNominatedMovieEvent
{
    public string MovieId { get; set; } = null!;
    public string MovieName { get; set; } = null!;
    public int MovieCreationYear { get; set; }

    public string UserId { get; init; } = null!;

    public NominatedMovieEvent() {}

    public NominatedMovieEvent(IReadOnlyNominatedMovieEvent other) : base(other)
    {
        MovieId = other.MovieId;
        MovieName = other.MovieName;
        MovieCreationYear = other.MovieCreationYear;
        UserId = other.UserId;
    }
}

public readonly record struct NominatedEventRecord(string MovieId, string MovieName, int MovieCreationYear, string id, DateTime Created, int TenantId, string UserId) : IReadOnlyNominatedMovieEvent
{
    public NominatedEventRecord(IReadOnlyEmbeddedMovie movie, string id, DateTime created, int tenantId, string userId) : this(movie.id, movie.Name, movie.MovieCreationYear, id, created, tenantId, userId) { }
}