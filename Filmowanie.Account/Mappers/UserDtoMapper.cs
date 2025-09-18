using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Mappers;

internal sealed class UserDtoMapper : IUserDtoMapper
{
    private readonly ILogger<UserDtoMapper> _log;

    public UserDtoMapper(ILogger<UserDtoMapper> log)
    {
        _log = log;
    }

    public Maybe<UserDTO> Map(Maybe<DomainUser> maybeUser) => maybeUser.Accept(MapToDtoInternal, _log);

    private static Maybe<UserDTO> MapToDtoInternal(DomainUser user)
    {
        var userDto = new UserDTO(user.Name, user.IsAdmin, user.HasBasicAuthSetup);
        return new Maybe<UserDTO>(userDto, null);
    }
}
