using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedUser : IReadOnlyEmbeddedUser
{
    public string id { get; set; }

    public string Name { get; set; }

    public int TenantId { get; set; }

    public EmbeddedUser() { }

    public EmbeddedUser(IReadOnlyEmbeddedUser user) : this()
    {
        id = user.id;
        Name = user.Name;
        TenantId = user.TenantId;
    }
}