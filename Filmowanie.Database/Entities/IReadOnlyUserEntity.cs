using Filmowanie.Database.Interfaces;

namespace Filmowanie.Database.Entities;

public interface IReadOnlyUserEntity : IReadOnlyEntity
{
    public string Email { get; }

    public string PasswordHash { get; set; }

    public string Code { get; set; }

    public string Username { get; set; }

    public int TenantId { get; set; }

    public bool IsAdmin { get; set; }
}