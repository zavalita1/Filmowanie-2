using Filmowanie.Abstractions;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IUsersCommandRepository
{
    public Task<IReadOnlyUserEntity> UpdatePasswordAndMail(string id, BasicAuth newData, CancellationToken cancellationToken);

    public Task Insert(IReadOnlyUserEntity entity, CancellationToken cancellation);
}