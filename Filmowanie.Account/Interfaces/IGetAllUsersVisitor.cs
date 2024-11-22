using Filmowanie.Abstractions;

namespace Filmowanie.Account.Interfaces;

internal interface IGetAllUsersVisitor : IOperationAsyncVisitor<IEnumerable<DomainUser>>;