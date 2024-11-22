using Filmowanie.Abstractions;
using Filmowanie.Account.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

internal interface IEnrichUserVisitor : IOperationAsyncVisitor<string, DetailedUserDTO>;