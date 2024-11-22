using Filmowanie.Abstractions;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
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

    public async Task<IReadOnlyUserEntity> UpdatePasswordAndMail(string id, BasicAuth newData, CancellationToken cancellationToken)
    {
        var user = await _identityDbContext.Users.SingleAsync(x => x.Code == id, cancellationToken);
        
        user.PasswordHash = newData.Password;
        user.Email = newData.Email;

        await _identityDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task Insert(IReadOnlyUserEntity entity, CancellationToken cancellation)
    {
        var userEntity = new UserEntity(entity);
        _identityDbContext.Users.Add(userEntity);
        await _identityDbContext.SaveChangesAsync(cancellation);
    }
}