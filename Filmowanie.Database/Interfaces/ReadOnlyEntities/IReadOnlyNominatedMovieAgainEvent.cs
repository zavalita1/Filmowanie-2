namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyNominatedMovieAgainEvent : IReadOnlyEntity
{
    public IReadOnlyEmbeddedMovie Movie { get; }
}