using Filmowanie.Abstractions;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

public interface ISignUpVisitor : IOperationAsyncVisitor<(DomainUser, BasicAuth), LoginResultData>;