namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyEmbeddedUser
{
   public string id { get; }
   public string Name { get; }

   public int TenantId { get; }
}

public interface IReadOnlyEmbeddedMovie
{
    public string id { get; }
    public string Name { get; }
}