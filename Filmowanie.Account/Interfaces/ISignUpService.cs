using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface ISignUpService
{
    Task<Maybe<LoginResultData>> SignUp(Maybe<(DomainUser, BasicAuth)> data, CancellationToken cancellation);
}