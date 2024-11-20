#nullable enable
using System.Linq.Expressions;
using Filmowanie.Database.Entities;

namespace Filmowanie.Account.Interfaces;

public interface IUsersQueryRepository
{
    Task<UserEntity?> GetUserAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken cancellationToken);
}