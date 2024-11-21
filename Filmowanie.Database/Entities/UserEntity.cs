namespace Filmowanie.Database.Entities;

public class UserEntity : Entity, IReadOnlyUserEntity
{
    public virtual string Email { get; set; }

    public virtual string PasswordHash { get; set; }

    public virtual string Code { get; set; }

    public virtual string Username { get; set; }

    public virtual int TenantId { get; set; }

    public virtual bool IsAdmin { get; set; }

    public UserEntity()
    { }

    public UserEntity(IReadOnlyUserEntity other)
    {
        Email = other.Email;
        PasswordHash = other.PasswordHash;
        Code = other.Code;
        Username = other.Username;
        TenantId = other.TenantId;
        IsAdmin = other.IsAdmin;
        id = other.Id;
        Created = other.Created;
    }
}
