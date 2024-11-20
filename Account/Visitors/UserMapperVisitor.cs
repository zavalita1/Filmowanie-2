using Filmowanie.Abstractions;
using Filmowanie.Account.Interfaces;
using Filmowanie.DTOs.Outgoing;
using Filmowanie.Interfaces;

namespace Filmowanie.Account.Visitors;

public sealed class UserMapperVisitor : IUserMapperVisitor
{
    public OperationResult<UserDTO> Visit(OperationResult<DomainUser> user)
    {
        var userDto = new UserDTO(user.Result.Username, user.Result.IsAdmin, user.Result.HasBasicAuthSetup);
        return new OperationResult<UserDTO>(userDto, null);
    }
}