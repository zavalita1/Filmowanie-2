namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyNominatedMovieEvent : IReadOnlyEntity
{
    public string MovieId { get; }
    public string MovieName { get; }
    public int MovieCreationYear { get; }

    public string UserId { get; }
}