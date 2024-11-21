using Filmowanie.Abstractions;
using Filmowanie.Database.Entities;

namespace Filmowanie.Database.Interfaces;

public interface IUsersCommandRepository
{
    public Task<IReadOnlyUserEntity> UpdatePasswordAndMail(string id, BasicAuth newData, CancellationToken cancellationToken);

    public Task Insert(IReadOnlyUserEntity entity, CancellationToken cancellation);
}