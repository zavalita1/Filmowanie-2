#nullable enable
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Database.Entities;

namespace Filmowanie.Account.Interfaces;

public interface IUsersQueryRepository
{
    Task<UserEntity?> GetUserAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken cancellationToken);
}