using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class CanNominateMovieAgainEvent : Event, IReadOnlyCanNominateMovieAgainEvent
{
    public EmbeddedMovie Movie { get; set; } = null!;

    IReadOnlyEmbeddedMovie IReadOnlyCanNominateMovieAgainEvent.Movie => Movie;

    public CanNominateMovieAgainEvent()
    { }

    public CanNominateMovieAgainEvent(IReadOnlyCanNominateMovieAgainEvent other) : base(other)
    {
        Movie = other.Movie.AsMutable();
    }
}

public readonly record struct CanNominateMovieAgainEventRecord(IReadOnlyEmbeddedMovie Movie, string id, DateTime Created, int TenantId) : IReadOnlyCanNominateMovieAgainEvent;
