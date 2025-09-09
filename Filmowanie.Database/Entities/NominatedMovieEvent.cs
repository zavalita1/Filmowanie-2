using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class NominatedMovieEvent : Event, IReadOnlyNominatedMovieEvent
{
    public EmbeddedMovie Movie { get; set; }

    IReadOnlyEmbeddedMovie IReadOnlyNominatedMovieEvent.Movie => Movie;

    public string UserId { get; init; }

    public NominatedMovieEvent() {}

    public NominatedMovieEvent(IReadOnlyNominatedMovieEvent other) : base(other)
    {
        Movie = other.Movie.AsMutable();
        UserId = other.UserId;
    }
}

public readonly record struct NominatedEventRecord(IReadOnlyEmbeddedMovie Movie, string id, DateTime Created, int TenantId, string UserId) : IReadOnlyNominatedMovieEvent;