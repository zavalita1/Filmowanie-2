namespace Filmowanie.Database.Interfaces;

public interface IReadOnlyEntity
{
    public string Id { get; }

    public DateTime Created { get; set; }
}