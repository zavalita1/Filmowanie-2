using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface IDomainUserAccessor
{
    Maybe<DomainUser> GetDomainUser(Maybe<VoidResult> maybe);
}