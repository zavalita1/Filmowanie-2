using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface IAuthenticationManager : ICurrentUserAccessor
{
    Task<Maybe<VoidResult>> LogInAsync(Maybe<LoginResultData> maybeLoginData, CancellationToken cancelToken);
    Task<Maybe<VoidResult>> LogOutAsync(Maybe<VoidResult> maybe, CancellationToken cancelToken);
}