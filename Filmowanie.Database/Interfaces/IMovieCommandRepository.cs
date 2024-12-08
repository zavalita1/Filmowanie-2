using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IMovieCommandRepository
{
    Task InsertCanBeNominatedAgainAsync(IEnumerable<IReadOnlyCanNominateMovieAgainEvent> canNominateMovieAgainEvents, CancellationToken cancellationToken);
    Task InsertNominatedAgainAsync(IReadOnlyNominatedMovieAgainEvent nominatedAgainEvent, CancellationToken cancellationToken);
    Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancellationToken);
}