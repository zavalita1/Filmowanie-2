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

    public virtual string id { get; set; } = null!;

    public DateTime Created { get; set; }
    public int TenantId { get; set; }
}