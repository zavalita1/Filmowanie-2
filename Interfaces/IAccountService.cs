using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Abstractions;
using Filmowanie.Account.Results;

namespace Filmowanie.Interfaces;

public interface IAccountService
{
    Task<OperationResult<LoginResultData>> LoginAsync(OperationResult<string> code, CancellationToken cancellation);
    Task<OperationResult<LoginResultData>> LoginAsync(OperationResult<BasicAuth> data, CancellationToken cancellation);
    Task<OperationResult<LoginResultData>> SignUpAsync(OperationResult<BasicAuth> data, CancellationToken cancellation);
}