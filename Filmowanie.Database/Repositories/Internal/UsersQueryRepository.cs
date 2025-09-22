using System.Linq.Expressions;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories.Internal;

internal sealed class UsersQueryRepository : IUsersQueryRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public UsersQueryRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    public async Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<IReadOnlyUserEntity, bool>> predicate, CancellationToken cancelToken)
    {
        return await _identityDbContext.Users.SingleOrDefaultAsync(predicate, cancelToken);
    }

    public async Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancelToken)
    {
        var result = await _identityDbContext.Users
            .Where(x => x.Type == nameof(UserEntity)) // cosmos emulator incorrectly handles types discrimination, this helps.
            .ToArrayAsync(cancelToken);
        return result.Cast<IReadOnlyUserEntity>().ToArray();
    }
}