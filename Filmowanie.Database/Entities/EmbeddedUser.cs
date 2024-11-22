using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedUser : IReadOnlyEmbeddedUser
{
    public string id { get; set; }

    public string Name { get; set; }
}