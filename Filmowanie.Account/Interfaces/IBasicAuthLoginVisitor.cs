using Filmowanie.Abstractions;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface IBasicAuthLoginVisitor : IOperationAsyncVisitor<BasicAuth, LoginResultData>;