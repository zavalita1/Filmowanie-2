using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface ISignUpService
{
    Task<OperationResult<LoginResultData>> SignUp(OperationResult<(DomainUser, BasicAuth)> data, CancellationToken cancellation);
}