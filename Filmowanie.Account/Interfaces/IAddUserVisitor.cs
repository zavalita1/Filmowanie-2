using Filmowanie.Abstractions;

namespace Filmowanie.Account.Interfaces;

internal interface IAddUserVisitor : IOperationAsyncVisitor<DomainUser, object>;