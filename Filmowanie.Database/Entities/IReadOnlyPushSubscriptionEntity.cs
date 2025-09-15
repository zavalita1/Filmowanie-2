using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public sealed class ReadOnlyPushSubscriptionEntity : Entity, IReadOnlyPushSubscriptionEntity
{
    public string P256DH { get; set; }
    public string Auth { get; set; }
    public string Endpoint { get; set; }

    public EmbeddedUser User { get; set; }

    IReadOnlyEmbeddedUser IReadOnlyPushSubscriptionEntity.User => User;

    internal ReadOnlyPushSubscriptionEntity() { }

    public ReadOnlyPushSubscriptionEntity(IReadOnlyPushSubscriptionEntity other)
    {
        P256DH = other.P256DH;
        Auth = other.Auth;
        Endpoint = other.Endpoint;
        id = other.id;
        Created = other.Created;
        TenantId = other.TenantId;
        User = new EmbeddedUser(other.User);
    }
}