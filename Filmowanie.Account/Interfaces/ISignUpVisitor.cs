using Filmowanie.Abstractions;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface ISignUpVisitor : IOperationAsyncVisitor<(DomainUser, BasicAuth), LoginResultData>;