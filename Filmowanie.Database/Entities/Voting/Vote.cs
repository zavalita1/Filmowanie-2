using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting;

public class Vote : IReadOnlyVote
{
    public virtual EmbeddedUser User { get; set; } = null!;
    public virtual VoteType VoteType { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyVote.User => User;

    public Vote() { }

    public Vote(IReadOnlyVote vote)
    {
        VoteType = vote.VoteType;
        User = vote.User.AsMutable();
    }
}