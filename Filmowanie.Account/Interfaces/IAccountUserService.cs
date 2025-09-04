using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface IAccountUserService
{
    Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<string> maybeCode, CancellationToken cancelToken);
    Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<BasicAuth> maybeBasicAuthData, CancellationToken cancelToken);

    Task<Maybe<IEnumerable<DomainUser>>> GetAllUsers(Maybe<VoidResult> maybe, CancellationToken cancelToken);

    Task<Maybe<DetailedUserDTO>> GetByIdAsync(Maybe<string> maybeId, CancellationToken cancellationToken);

    Task<Maybe<VoidResult>> AddUserAsync(Maybe<DomainUser> input, CancellationToken cancellationToken);
}