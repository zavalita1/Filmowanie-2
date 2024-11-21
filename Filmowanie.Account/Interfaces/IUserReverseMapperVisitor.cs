using Filmowanie.Abstractions;
using Filmowanie.Account.DTOs.Incoming;

namespace Filmowanie.Account.Interfaces;

public interface IUserReverseMapperVisitor : IOperationVisitor<(UserDTO, DomainUser CurrentUser), DomainUser>;