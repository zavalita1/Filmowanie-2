using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IReadOnlyMoviesThatCanBeNominatedAgainEntity : IReadOnlyEntity
{
    public IReadOnlyEmbeddedMovie Movie { get; }
}