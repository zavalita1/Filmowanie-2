using Filmowanie.Abstractions;
using Filmowanie.Account.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

internal interface IUserMapperVisitor : IOperationVisitor<DomainUser, UserDTO>;