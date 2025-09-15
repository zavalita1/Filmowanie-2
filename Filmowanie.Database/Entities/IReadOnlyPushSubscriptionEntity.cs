using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public sealed class ReadOnlyPushSubscriptionEntity : Entity, IReadOnlyPushSubscriptionEntity
{
    public string P256DH { get; set; } = null!;
    public string Auth { get; set; } = null!;
    public string Endpoint { get; set; } = null!;

    public EmbeddedUser User { get; set; } = null!;

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