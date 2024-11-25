namespace Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;

public interface IVotingStartedEvent : IReadOnlyEntity
{
    IReadOnlyMovieEntity[] Movies { get; }

    string VotingId { get;  }
}