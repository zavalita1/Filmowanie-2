using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class UserReverseMapperVisitor : IUserReverseMapperVisitor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<UserReverseMapperVisitor> _log;

    public UserReverseMapperVisitor(IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider, ILogger<UserReverseMapperVisitor> log)
    {
        _dateTimeProvider = dateTimeProvider;
        _guidProvider = guidProvider;
        _log = log;
    }

    public OperationResult<DomainUser> Visit(OperationResult<(DTOs.Incoming.UserDTO, DomainUser CurrentUser)> input)
    {
        var now = _dateTimeProvider.Now;
        var guid = _guidProvider.NewGuid();
        var userId = $"user-{guid}";
        var domainUser = new DomainUser(userId, input.Result.Item1.Id, false, false, input.Result.CurrentUser.Tenant, now);
        return new OperationResult<DomainUser>(domainUser, null);
    }

    public ILogger Log => _log;
}