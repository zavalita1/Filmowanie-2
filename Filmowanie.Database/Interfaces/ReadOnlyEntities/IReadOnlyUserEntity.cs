namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyUserEntity : IReadOnlyEntity
{
    public string Email { get; }

    public string PasswordHash { get; set; }

    public string Code { get; set; }

    public string DisplayName { get; set; }

    public int TenantId { get; set; }

    public bool IsAdmin { get; set; }
}