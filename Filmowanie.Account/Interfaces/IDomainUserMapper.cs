using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Account.Interfaces;

internal interface IDomainUserMapper
{
    Maybe<DomainUser> Map(Maybe<(DTOs.Incoming.UserDTO, DomainUser CurrentUser)> maybe);
}
