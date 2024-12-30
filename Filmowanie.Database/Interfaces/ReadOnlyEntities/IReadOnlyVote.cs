using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyVote
{
    public VoteType VoteType { get; }

    public IReadOnlyEmbeddedUser User { get; }

}