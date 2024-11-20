using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Account.Repositories;

#nullable enable

public class UsersQueryRepository : IUsersQueryRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public UsersQueryRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    public Task<UserEntity?> GetUserAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken cancellationToken) => _identityDbContext.Users.SingleOrDefaultAsync(predicate, cancellationToken);
}