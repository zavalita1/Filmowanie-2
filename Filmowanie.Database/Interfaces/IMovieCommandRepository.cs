using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IMovieCommandRepository
{
    public Task DeleteEventsForMovieAsync(MovieId movieId, CancellationToken cancelToken);
    public Task InsertCanBeNominatedAgainAsync(IEnumerable<IReadOnlyCanNominateMovieAgainEvent> canNominateMovieAgainEvents, CancellationToken cancelToken);
    public Task InsertNominatedAsync(IReadOnlyNominatedMovieEvent nominatedEvent, CancellationToken cancelToken);
    public Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancelToken);

    public Task UpdatePosterAsync(string entityId, string posterUrl, string bigPosterUrl, CancellationToken cancelToken);

    public Task<IReadOnlyMovieEntity> UpdateAltDescriptionAsync(string entityId, string altDescription, CancellationToken cancelToken);

    public Task<IReadOnlyMovieEntity> MarkMovieAsRejectedAsync(string entityId, CancellationToken cancelToken);
}