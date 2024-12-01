using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting;

public class Vote : IReadOnlyVote
{
    public virtual EmbeddedUser User { get; set; } 
    public virtual VoteType VoteType { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyVote.User => User;
}

public interface IReadOnlyVote
{
    public VoteType VoteType { get; }

    public IReadOnlyEmbeddedUser User { get; }

}