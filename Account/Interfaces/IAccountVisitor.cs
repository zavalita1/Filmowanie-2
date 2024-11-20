using Filmowanie.Abstractions;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

public interface ICodeLoginVisitor : IOperationAsyncVisitor<string, LoginResultData>;
public interface IBasicAuthLoginVisitor : IOperationAsyncVisitor<BasicAuth, LoginResultData>;
public interface ISignUpVisitor : IOperationAsyncVisitor<(DomainUser, BasicAuth), LoginResultData>;
