namespace Filmowanie.Database.Entities;

public class UserEntity : Entity
{
    public virtual string Email { get; set; }

    public virtual string PasswordHash { get; set; }

    public virtual string Code { get; set; }

    public virtual string Username { get; set; }

    public virtual int TenantId { get; set; }

    public virtual bool IsAdmin { get; set; }
}
