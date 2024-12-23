using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Account.Interfaces;

internal interface IGetAllUsersVisitor : IOperationAsyncVisitor<IEnumerable<DomainUser>>;