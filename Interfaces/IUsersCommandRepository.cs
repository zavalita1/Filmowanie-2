using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Abstractions;

namespace Filmowanie.Interfaces;

public interface IUsersCommandRepository
{
    public Task UpdatePasswordAndMail(string id, BasicAuth newData, CancellationToken cancellationToken)
}