using Filmowanie.Abstractions;

namespace Filmowanie.Interfaces;

public interface IUserIdentityService
{
    OperationResult<DomainUser> GetCurrentUser<T>(OperationResult<T> operationResult);
}