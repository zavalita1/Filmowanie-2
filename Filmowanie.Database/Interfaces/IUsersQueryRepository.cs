#nullable enable
using System.Linq.Expressions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IUsersQueryRepository
{
    public Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<IReadOnlyUserEntity, bool>> predicate, CancellationToken cancelToken);

    public Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancelToken);
}