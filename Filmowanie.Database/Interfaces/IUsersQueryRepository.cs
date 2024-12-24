#nullable enable
using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IUsersQueryRepository
{
    public Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<IReadOnlyUserEntity, bool>> predicate, CancellationToken cancellationToken);

    public Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancellationToken);
}