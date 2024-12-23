using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface ISignUpVisitor : IOperationAsyncVisitor<(DomainUser, BasicAuth), LoginResultData>;