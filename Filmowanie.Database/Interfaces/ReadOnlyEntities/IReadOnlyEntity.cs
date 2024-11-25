namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyEntity
{
    public string Id { get; }

    public DateTime Created { get; set; }

    public int TenantId { get; set; }
}