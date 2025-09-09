using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Abstractions.Interfaces;

public interface ICurrentUserAccessor
{
    Maybe<DomainUser> GetDomainUser(Maybe<VoidResult> maybe);
}