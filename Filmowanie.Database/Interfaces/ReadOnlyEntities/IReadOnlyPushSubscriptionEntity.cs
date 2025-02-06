namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyPushSubscriptionEntity : IReadOnlyEntity
{
    string P256DH { get; }
    string Auth { get; }
    string Endpoint { get; }

    IReadOnlyEmbeddedUser User { get; }
}