using Filmowanie.Abstractions;
using Filmowanie.DTOs.Outgoing;

namespace Filmowanie.Interfaces;

public interface IUserMapper
{
    OperationResult<UserDTO> Map(OperationResult<DomainUser> user);
}