using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public abstract class Entity : IReadOnlyEntity
{
    public virtual string Type
    {
        get => GetType().Name;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public string id { get; set; }

    public DateTime Created { get; set; }
    public string Id => id;

    public int TenantId { get; set; }
}