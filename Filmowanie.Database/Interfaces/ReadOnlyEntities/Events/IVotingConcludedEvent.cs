namespace Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;

public interface IVotingConcludedEvent : IReadOnlyEntity
{
    IReadOnlyMovieEntity[] Movies { get; }

    string VotingId { get; }
}