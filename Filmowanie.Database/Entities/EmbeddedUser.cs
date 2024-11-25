using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

internal class EmbeddedUser : IReadOnlyEmbeddedUser
{
    public string id { get; set; }

    public string Name { get; set; }

    public int TenantId { get; set; }
}

internal class EmbeddedMovie : IReadOnlyEmbeddedMovie
{
    public string id { get; set; }

    public string Name { get; set; }
}