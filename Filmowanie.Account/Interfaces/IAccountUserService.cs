using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Models;

namespace Filmowanie.Account.Interfaces;

internal interface IAccountUserService
{
    Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<Code> maybeCode, CancellationToken cancelToken);

    Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<BasicAuthUserData> maybeBasicAuthData, CancellationToken cancelToken);

    Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<GoogleUserData> maybeGoogleUserData, CancellationToken cancelToken);

    Task<Maybe<IEnumerable<DomainUser>>> GetAllUsers(Maybe<VoidResult> maybe, CancellationToken cancelToken);

    Task<Maybe<DetailedUserDTO>> GetByIdAsync(Maybe<string> maybeId, CancellationToken cancelToken);

    Task<Maybe<VoidResult>> AddUserAsync(Maybe<DomainUser> input, CancellationToken cancelToken);
}
