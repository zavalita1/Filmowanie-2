using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IMovieCommandRepository
{
    public Task InsertCanBeNominatedAgainAsync(IEnumerable<IReadOnlyCanNominateMovieAgainEvent> canNominateMovieAgainEvents, CancellationToken cancelToken);
    public Task InsertNominatedAsync(IReadOnlyNominatedMovieEvent nominatedEvent, CancellationToken cancelToken);
    public Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancelToken);

    public Task UpdateMovieAsync(string entityId, string posterUrl, CancellationToken cancelToken);
    public Task<IReadOnlyMovieEntity> MarkMovieAsRejectedAsync(string entityId, CancellationToken cancelToken);
}