using Filmowanie.Abstractions;
using Filmowanie.Account.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

public interface IEnrichUserVisitor : IOperationAsyncVisitor<string, DetailedUserDTO>;