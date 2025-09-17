using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting;

public class Vote : IReadOnlyVote
{
    //public virtual string? id { get; set; }

    public virtual EmbeddedUser User { get; set; } = null!;
    public virtual VoteType VoteType { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyVote.User => User;

    public Vote()
    {
      // id = Guid.NewGuid().ToString();
    }

    public Vote(IReadOnlyVote vote)
    {
        VoteType = vote.VoteType;
        User = vote.User.AsMutable();
        //id = vote.id;
    }
}