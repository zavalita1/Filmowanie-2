using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Incoming;

namespace Filmowanie.Account.Interfaces;

internal interface IUserReverseMapperVisitor : IOperationVisitor<(UserDTO, DomainUser CurrentUser), DomainUser>;