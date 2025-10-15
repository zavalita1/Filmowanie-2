using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Incoming;

namespace Filmowanie.Account.Interfaces;

internal interface IDomainUserMapper
{
    Maybe<DomainUser> Map(Maybe<CreateUserDTO> maybeDto, Maybe<DomainUser> maybeUser);
}
