namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyCanNominateMovieAgainEvent : IReadOnlyEntity
{
    public IReadOnlyEmbeddedMovie Movie { get; }
}