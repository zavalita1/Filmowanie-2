using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class MoviesThatCanBeNominatedAgainEntity : Entity, IReadOnlyMoviesThatCanBeNominatedAgainEntity
{
    public override string id { get => $"{nameof(MoviesThatCanBeNominatedAgainEntity)}-{TenantId}"; set { } }

    public IEnumerable<EmbeddedMovie> Movies { get; set; }

    IEnumerable<IReadOnlyEmbeddedMovie> IReadOnlyMoviesThatCanBeNominatedAgainEntity.Movies => Movies;
}