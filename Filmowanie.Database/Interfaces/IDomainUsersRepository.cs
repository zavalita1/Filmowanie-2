using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IDomainUsersRepository
{
    Task<IReadOnlyUserEntity?> GetUserByIdAsync(string id, CancellationToken cancelToken);
    Task<IReadOnlyUserEntity?> GetUserByCodeAsync(string code, CancellationToken cancelToken);
    Task<IReadOnlyUserEntity?> GetUserByMailAsync(string mail, CancellationToken cancelToken);
    Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancelToken);
}