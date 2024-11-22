using System.Linq.Expressions;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

#nullable enable

public class UsersQueryRepository : IUsersQueryRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public UsersQueryRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    public async Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _identityDbContext.Users.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _identityDbContext.Users.ToArrayAsync(cancellationToken);
        return result.Cast<IReadOnlyUserEntity>().ToArray();
    }
}