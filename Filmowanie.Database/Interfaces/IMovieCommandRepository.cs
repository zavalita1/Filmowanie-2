using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IMovieCommandRepository
{
    public Task InsertCanBeNominatedAgainAsync(IEnumerable<IReadOnlyCanNominateMovieAgainEvent> canNominateMovieAgainEvents, CancellationToken cancellationToken);
    public Task InsertNominatedAgainAsync(IReadOnlyNominatedMovieAgainEvent nominatedAgainEvent, CancellationToken cancellationToken);
    public Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancellationToken);

    public Task UpdateMovieAsync(string entityId, string posterUrl, CancellationToken cancellationToken);
}