using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class NominatedMovieAgainEvent : Event, IReadOnlyNominatedMovieAgainEvent
{
    public EmbeddedMovie Movie { get; set; }

    IReadOnlyEmbeddedMovie IReadOnlyNominatedMovieAgainEvent.Movie => Movie;

    public NominatedMovieAgainEvent() {}

    public NominatedMovieAgainEvent(IReadOnlyNominatedMovieAgainEvent other) : base(other)
    {
        Movie = other.Movie.AsMutable();
    }
}

public readonly record struct NominatedMovieAgainEventRecord(IReadOnlyEmbeddedMovie Movie, string id, DateTime Created, int TenantId) : IReadOnlyNominatedMovieAgainEvent;