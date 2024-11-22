using Filmowanie.Abstractions;
using Filmowanie.Account.Results;

namespace Filmowanie.Account.Interfaces;

internal interface ICodeLoginVisitor : IOperationAsyncVisitor<string, LoginResultData>;