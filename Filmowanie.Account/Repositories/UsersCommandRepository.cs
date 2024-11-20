using Filmowanie.Abstractions;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Account.Repositories;

public sealed class UsersCommandRepository : IUsersCommandRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public UsersCommandRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    public async Task<UserEntity> UpdatePasswordAndMail(string id, BasicAuth newData, CancellationToken cancellationToken)
    {
        var user = await _identityDbContext.Users.SingleAsync(x => x.Code == id, cancellationToken);
        
        user.PasswordHash = newData.Password;
        user.Email = newData.Email;

        await _identityDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }
}