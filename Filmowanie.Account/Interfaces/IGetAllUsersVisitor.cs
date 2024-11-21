using Filmowanie.Abstractions;

namespace Filmowanie.Account.Interfaces;

public interface IGetAllUsersVisitor : IOperationAsyncVisitor<IEnumerable<DomainUser>>;
public interface IAddUserVisitor : IOperationAsyncVisitor<DomainUser, object>;