using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class CanNominateMovieAgainEvent : Event, IReadOnlyCanNominateMovieAgainEvent
{
    public EmbeddedMovie Movie { get; set; }

    IReadOnlyEmbeddedMovie IReadOnlyCanNominateMovieAgainEvent.Movie => Movie;

    public CanNominateMovieAgainEvent()
    { }

    public CanNominateMovieAgainEvent(IReadOnlyCanNominateMovieAgainEvent other) : base(other)
    {
        Movie = new EmbeddedMovie(other.Movie);
    }
}

internal class NominatedMovieAgainEvent : Event, IReadOnlyNominatedMovieAgainEvent
{
    public EmbeddedMovie Movie { get; set; }

    IReadOnlyEmbeddedMovie IReadOnlyNominatedMovieAgainEvent.Movie => Movie;

    public NominatedMovieAgainEvent() {}

    public NominatedMovieAgainEvent(IReadOnlyNominatedMovieAgainEvent other) : base(other)
    {
        Movie = new EmbeddedMovie(other.Movie);
    }
}

public interface IReadOnlyCanNominateMovieAgainEvent : IReadOnlyEntity
{
    public IReadOnlyEmbeddedMovie Movie { get; }
}

public interface IReadOnlyNominatedMovieAgainEvent : IReadOnlyEntity
{
    public IReadOnlyEmbeddedMovie Movie { get; }
}

public readonly record struct CanNominateMovieAgainEventRecord(IReadOnlyEmbeddedMovie Movie, string Id, DateTime Created, int TenantId) : IReadOnlyCanNominateMovieAgainEvent;
public readonly record struct NominatedMovieAgainEventRecord(IReadOnlyEmbeddedMovie Movie, string Id, DateTime Created, int TenantId) : IReadOnlyNominatedMovieAgainEvent;