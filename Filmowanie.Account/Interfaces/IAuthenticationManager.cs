using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface IAuthenticationManager : IDomainUserAccessor
{
    Task<Maybe<VoidResult>> LogInAsync(Maybe<LoginResultData> maybeLoginData, CancellationToken cancelToken);
    Task<Maybe<VoidResult>> LogOutAsync(Maybe<VoidResult> maybe, CancellationToken cancelToken);
}