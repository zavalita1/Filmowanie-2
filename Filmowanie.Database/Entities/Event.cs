using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public abstract class Event : IReadOnlyEntity
{
    public virtual string Type
    {
        get => GetType().Name;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public virtual string id { get; set; }

    public DateTime Created { get; set; }

    public int TenantId { get; set; }

    protected Event()
    {

    }

    protected Event(IReadOnlyEntity entity)
    {
        id = entity.id;
        Created = entity.Created;
        TenantId = entity.TenantId;
    }
}