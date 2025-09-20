using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Models;

namespace Filmowanie.Account.Interfaces;

internal interface IGoogleAuthService
{
    Task<Maybe<GoogleUserData>> GetUserData(Maybe<GoogleCode> maybeToken, CancellationToken cancelToken);
}