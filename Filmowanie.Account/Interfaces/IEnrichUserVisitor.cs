using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

internal interface IEnrichUserVisitor : IOperationAsyncVisitor<string, DetailedUserDTO>;