using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyEmbeddedUserWithNominationAward 
{
    public IReadOnlyEmbeddedUser User {get;}
    public string AwardMessage { get; }

    public Decade Decade { get; }
}