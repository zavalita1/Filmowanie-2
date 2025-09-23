using System.Linq.Expressions;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories.Internal;

internal sealed class UsersQueryRepository : IUsersQueryRepository
{
    private readonly IdentityDbContext identityDbContext;

    public UsersQueryRepository(IdentityDbContext identityDbContext)
    {
        this.identityDbContext = identityDbContext;
    }

    public async Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<IReadOnlyUserEntity, bool>> predicate, CancellationToken cancelToken)
    {
        return await this.identityDbContext.Users.SingleOrDefaultAsync(predicate, cancelToken);
    }

    public async Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancelToken)
    {
        var result = await this.identityDbContext.Users
            .Where(x => x.Type == nameof(UserEntity)) // cosmos emulator incorrectly handles types discrimination, this helps.
            .ToArrayAsync(cancelToken);
        return result.Cast<IReadOnlyUserEntity>().ToArray();
    }
}