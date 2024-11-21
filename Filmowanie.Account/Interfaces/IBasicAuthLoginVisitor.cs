using Filmowanie.Abstractions;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

public interface IBasicAuthLoginVisitor : IOperationAsyncVisitor<BasicAuth, LoginResultData>;