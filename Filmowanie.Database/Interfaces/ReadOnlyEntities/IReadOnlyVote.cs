using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyVote
{
    //public string? id { get;  }

    public VoteType VoteType { get; }

    public IReadOnlyEmbeddedUser User { get; }

}