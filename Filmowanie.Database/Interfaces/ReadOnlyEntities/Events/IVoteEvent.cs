using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;

public interface IVoteAddedEvent : IReadOnlyEntity
{
    public VoteType VoteType { get; }

    public string VotingId { get;  }

    public IReadOnlyEmbeddedUser User { get; }

    public IReadOnlyEmbeddedMovie Movie { get; }
}

public interface IVoteRemovedEvent : IReadOnlyEntity
{
    public VoteType VoteType { get; }

    public string VotingId { get; }

    public IReadOnlyEmbeddedUser User { get; }

    public IReadOnlyEmbeddedMovie Movie { get; }
}