using Filmowanie.Abstractions;
using Filmowanie.Account.DTOs.Incoming;

namespace Filmowanie.Account.Interfaces;

internal interface IUserReverseMapperVisitor : IOperationVisitor<(UserDTO, DomainUser CurrentUser), DomainUser>;