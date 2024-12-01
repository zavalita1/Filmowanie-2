using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedUser : IReadOnlyEmbeddedUser
{
    public string id { get; set; }

    public string Name { get; set; }

    public int TenantId { get; set; }
}

public class EmbeddedUserWithNominationAward : EmbeddedUser, IReadOnlyEmbeddedUserWithNominationAward
{
    public string AwardMessage { get; set; }

    public Decade Decade { get; set; }
}

public interface IReadOnlyEmbeddedUserWithNominationAward : IReadOnlyEmbeddedUser
{
    public string AwardMessage { get; }

    public Decade Decade { get; }
}