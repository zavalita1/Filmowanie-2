using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface IAuthenticationManager
{
    Task<OperationResult<VoidResult>> LogInAsync(OperationResult<LoginResultData> maybeLoginData, CancellationToken cancelToken);
    Task<OperationResult<VoidResult>> LogOutAsync(OperationResult<VoidResult> maybe, CancellationToken cancelToken);
    OperationResult<DomainUser> GetDomainUser(OperationResult<VoidResult> maybe);
}