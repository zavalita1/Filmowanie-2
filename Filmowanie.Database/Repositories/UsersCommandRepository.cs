using Filmowanie.Database.Contexts;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal sealed class UsersCommandRepository : IUsersCommandRepository
{
    private readonly IdentityDbContext identityDbContext;

    public UsersCommandRepository(IdentityDbContext identityDbContext)
    {
        this.identityDbContext = identityDbContext;
    }

    public async Task<IReadOnlyUserEntity> UpdatePasswordAndMail(string id, (string Mail, string Password) data, CancellationToken cancelToken)
    {
        var user = await this.identityDbContext.Users.SingleAsync(x => x.Code == id, cancelToken);
        
        user.PasswordHash = data.Password;
        user.Email = data.Mail;

        await this.identityDbContext.SaveChangesAsync(cancelToken);
        return user;
    }

    public async Task Insert(IReadOnlyUserEntity entity, CancellationToken cancellation)
    {
        var userEntity = entity.AsMutable();
        this.identityDbContext.Users.Add(userEntity);
        await this.identityDbContext.SaveChangesAsync(cancellation);
    }
}