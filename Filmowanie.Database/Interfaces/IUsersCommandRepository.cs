using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IUsersCommandRepository
{
    public Task<IReadOnlyUserEntity> UpdatePasswordAndMail(string id, BasicAuth newData, CancellationToken cancelToken);

    public Task Insert(IReadOnlyUserEntity entity, CancellationToken cancellation);
}