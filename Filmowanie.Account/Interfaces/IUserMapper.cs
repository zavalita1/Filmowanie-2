using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

internal interface IUserMapper
{
    OperationResult<UserDTO> Map(OperationResult<DomainUser> maybeUser);

    OperationResult<DomainUser> Map(OperationResult<(DTOs.Incoming.UserDTO, DomainUser CurrentUser)> maybe);
}