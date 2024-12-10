namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyEntity
{
    public string id { get; }

    public DateTime Created { get; }

    public int TenantId { get; }
}