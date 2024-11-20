using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Abstractions;
using Filmowanie.Database.Entities;

namespace Filmowanie.Account.Interfaces;

public interface IUsersCommandRepository
{
    public Task<UserEntity> UpdatePasswordAndMail(string id, BasicAuth newData, CancellationToken cancellationToken);
}