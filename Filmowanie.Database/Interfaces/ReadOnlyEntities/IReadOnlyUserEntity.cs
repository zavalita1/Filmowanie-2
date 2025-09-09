namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyUserEntity : IReadOnlyEntity
{
    public string Email { get; }

    public string PasswordHash { get; }

    public string Code { get; }

    public string DisplayName { get; }

    public bool IsAdmin { get; }
}