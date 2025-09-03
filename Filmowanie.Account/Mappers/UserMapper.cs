using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Mappers;

internal sealed class UserMapper : IUserMapper
{
    private readonly ILogger<UserMapper> _log;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;

    public UserMapper(ILogger<UserMapper> log, IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider)
    {
        _log = log;
        _dateTimeProvider = dateTimeProvider;
        _guidProvider = guidProvider;
    }

    public OperationResult<UserDTO> Map(OperationResult<DomainUser> maybeUser) => maybeUser.Accept(UserMapper.Map, _log);

    public OperationResult<DomainUser> Map(OperationResult<(DTOs.Incoming.UserDTO, DomainUser CurrentUser)> maybe) => maybe.Accept(Map, _log);

    private OperationResult<DomainUser> Map((DTOs.Incoming.UserDTO, DomainUser CurrentUser) input)
    {
        var now = _dateTimeProvider.Now;
        var guid = _guidProvider.NewGuid();
        var userId = $"user-{guid}";
        var domainUser = new DomainUser(userId, input.Item1.Id, false, false, input.CurrentUser.Tenant, now);
        return new OperationResult<DomainUser>(domainUser, null);
    }

    private static OperationResult<UserDTO> Map(DomainUser user)
    {
        var userDto = new UserDTO(user.Name, user.IsAdmin, user.HasBasicAuthSetup);
        return new OperationResult<UserDTO>(userDto, null);
    }
}