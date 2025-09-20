using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IUsersCommandRepository
{
    public Task<IReadOnlyUserEntity> UpdatePasswordAndMail(string id, (string Mail, string Password) data, CancellationToken cancelToken);

    public Task Insert(IReadOnlyUserEntity entity, CancellationToken cancellation);
}