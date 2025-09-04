using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface IDomainUserAccessor
{
    OperationResult<DomainUser> GetDomainUser(OperationResult<VoidResult> maybe);
}