using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Repositories;

internal sealed class DomainUsersRepository : IDomainUsersRepository
{
    private readonly IUsersQueryRepository _usersQueryRepository;

    public DomainUsersRepository(IUsersQueryRepository usersQueryRepository)
    {
        _usersQueryRepository = usersQueryRepository;
    }

    public Task<IReadOnlyUserEntity?> GetUserByIdAsync(string id, CancellationToken cancelToken) => _usersQueryRepository.GetUserAsync(x => x.id == id, cancelToken);
    public Task<IReadOnlyUserEntity?> GetUserByCodeAsync(string code, CancellationToken cancelToken) => _usersQueryRepository.GetUserAsync(x => x.Code == code, cancelToken);
    public Task<IReadOnlyUserEntity?> GetUserByMailAsync(string mail, CancellationToken cancelToken) => _usersQueryRepository.GetUserAsync(x => x.Email == mail, cancelToken);
    public Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancelToken) => _usersQueryRepository.GetAllAsync(cancelToken);
}