using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface IAccountUserService
{
    Task<OperationResult<LoginResultData>> GetUserIdentity(OperationResult<string> maybeCode, CancellationToken cancelToken);
    Task<OperationResult<LoginResultData>> GetUserIdentity(OperationResult<BasicAuth> maybeBasicAuthData, CancellationToken cancelToken);

    Task<OperationResult<IEnumerable<DomainUser>>> GetAllUsers(OperationResult<VoidResult> maybe, CancellationToken cancelToken);

    Task<OperationResult<DetailedUserDTO>> GetByIdAsync(OperationResult<string> maybeId, CancellationToken cancellationToken);

    Task<OperationResult<VoidResult>> AddUserAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken);
}