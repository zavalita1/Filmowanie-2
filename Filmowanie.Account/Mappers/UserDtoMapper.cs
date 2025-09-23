using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Mappers;

internal sealed class UserDtoMapper : IUserDtoMapper
{
    private readonly ILogger<UserDtoMapper> log;

    public UserDtoMapper(ILogger<UserDtoMapper> log)
    {
        this.log = log;
    }

    public Maybe<UserDTO> Map(Maybe<DomainUser> maybeUser) => maybeUser.Accept(MapToDtoInternal, this.log);

    private static Maybe<UserDTO> MapToDtoInternal(DomainUser user)
    {
        var gender = user.Gender.ToString();
        var userDto = new UserDTO(user.Name, user.IsAdmin, user.HasBasicAuthSetup, gender);
        return new Maybe<UserDTO>(userDto, null);
    }
}
