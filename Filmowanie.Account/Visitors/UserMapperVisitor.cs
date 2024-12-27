using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class UserMapperVisitor : IUserMapperVisitor
{
    private readonly ILogger<UserMapperVisitor> _log;

    public UserMapperVisitor(ILogger<UserMapperVisitor> log)
    {
        _log = log;
    }

    public OperationResult<UserDTO> Visit(OperationResult<DomainUser> user)
    {
        var userDto = new UserDTO(user.Result.Name, user.Result.IsAdmin, user.Result.HasBasicAuthSetup);
        return new OperationResult<UserDTO>(userDto, null);
    }

    public ILogger Log => _log;
}