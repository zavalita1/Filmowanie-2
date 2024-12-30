using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedUserWithNominationAward : IReadOnlyEmbeddedUserWithNominationAward
{
    public EmbeddedUser User { get; set; }

    public string AwardMessage { get; set; }

    public Decade Decade { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyEmbeddedUserWithNominationAward.User => User;

    public EmbeddedUserWithNominationAward()
    { }

    public EmbeddedUserWithNominationAward(IReadOnlyEmbeddedUserWithNominationAward user) : this()
    {
        User = user.User.AsMutable();
        AwardMessage = user.AwardMessage;
        Decade = user.Decade;
    }
}