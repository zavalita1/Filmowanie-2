using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;

namespace Filmowanie.Database.Entities.Events;

internal class VotingConcludedEvent : BaseEventEntity, IVotingConcludedEvent
{
    public MovieEntity[] Movies { get; set; }
    public string VotingId { get; set; }

    IReadOnlyMovieEntity[] IVotingConcludedEvent.Movies => Movies;
}