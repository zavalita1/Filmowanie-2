namespace Filmowanie.Database.Entities;

public abstract class Entity
{
    public virtual string Type
    {
        get => GetType().Name;
        // ReSharper disable once ValueParameterNotUsed
        set { }
    }

    public string id { get; set; }

    public DateTime Created { get; set; }
}