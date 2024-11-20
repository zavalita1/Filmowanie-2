using Filmowanie.Abstractions;
using Filmowanie.DTOs.Outgoing;
using Filmowanie.Interfaces;

namespace Filmowanie.Account.Mappers;

public sealed class UserMapper : IUserMapper
{
    public OperationResult<UserDTO> Map(OperationResult<DomainUser> user)
    {
        if (user.Error != null)
            return new OperationResult<UserDTO>(default, user.Error);

        var userDto = new UserDTO(user.Result.Username, user.Result.IsAdmin, user.Result.HasBasicAuthSetup);
        return new OperationResult<UserDTO>(userDto, null);
    }
}