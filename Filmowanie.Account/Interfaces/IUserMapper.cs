using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

internal interface IUserMapper
{
    Maybe<UserDTO> Map(Maybe<DomainUser> maybeUser);

    Maybe<DomainUser> Map(Maybe<(DTOs.Incoming.UserDTO, DomainUser CurrentUser)> maybe);
}