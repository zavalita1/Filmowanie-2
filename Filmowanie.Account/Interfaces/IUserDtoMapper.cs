using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

internal interface IUserDtoMapper
{
    Maybe<UserDTO> Map(Maybe<DomainUser> maybeUser);
}
