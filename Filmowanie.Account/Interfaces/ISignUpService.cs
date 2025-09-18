using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Models;

namespace Filmowanie.Account.Interfaces;

internal interface ISignUpService
{
    Task<Maybe<LoginResultData>> SignUp(Maybe<DomainUser> user, Maybe<BasicAuth> basicAuth, CancellationToken cancellation);
}