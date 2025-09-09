namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyNominatedMovieEvent : IReadOnlyEntity
{
    public IReadOnlyEmbeddedMovie Movie { get; }

    public string UserId { get; }
}