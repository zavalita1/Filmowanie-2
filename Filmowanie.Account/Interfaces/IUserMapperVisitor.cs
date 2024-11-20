using Filmowanie.Abstractions;
using Filmowanie.DTOs.Outgoing;

namespace Filmowanie.Account.Interfaces;

public interface IUserMapperVisitor : IOperationVisitor<DomainUser, UserDTO>;
