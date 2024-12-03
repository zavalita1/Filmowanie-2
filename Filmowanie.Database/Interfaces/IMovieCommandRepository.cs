using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IMovieCommandRepository
{
    Task UpdateMoviesThatCanBeNominatedAgainEntityAsync(string entityId, IEnumerable<IReadOnlyEmbeddedMovie> movies, CancellationToken cancellationToken);
    Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancellationToken);
}