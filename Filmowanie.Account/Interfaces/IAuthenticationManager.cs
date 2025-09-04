using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface IAuthenticationManager : IDomainUserAccessor
{
    Task<OperationResult<VoidResult>> LogInAsync(OperationResult<LoginResultData> maybeLoginData, CancellationToken cancelToken);
    Task<OperationResult<VoidResult>> LogOutAsync(OperationResult<VoidResult> maybe, CancellationToken cancelToken);
}