using Filmowanie.Database.Contexts;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal sealed class UsersCommandRepository : IUsersCommandRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public UsersCommandRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    public async Task<IReadOnlyUserEntity> UpdatePasswordAndMail(string id, (string Mail, string Password) data, CancellationToken cancelToken)
    {
        var user = await _identityDbContext.Users.SingleAsync(x => x.Code == id, cancelToken);
        
        user.PasswordHash = data.Password;
        user.Email = data.Mail;

        await _identityDbContext.SaveChangesAsync(cancelToken);
        return user;
    }

    public async Task Insert(IReadOnlyUserEntity entity, CancellationToken cancellation)
    {
        var userEntity = entity.AsMutable();
        _identityDbContext.Users.Add(userEntity);
        await _identityDbContext.SaveChangesAsync(cancellation);
    }
}