using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
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

    public Maybe<UserDTO> Map(Maybe<DomainUser> maybeUser) => maybeUser.Accept(UserMapper.Map, _log);

    public Maybe<DomainUser> Map(Maybe<(DTOs.Incoming.UserDTO, DomainUser CurrentUser)> maybe) => maybe.Accept(Map, _log);

    private Maybe<DomainUser> Map((DTOs.Incoming.UserDTO, DomainUser CurrentUser) input)
    {
        var now = _dateTimeProvider.Now;
        var guid = _guidProvider.NewGuid();
        var userId = $"user-{guid}";
        var domainUser = new DomainUser(userId, input.Item1.Id, false, false, input.CurrentUser.Tenant, now);
        return new Maybe<DomainUser>(domainUser, null);
    }

    private static Maybe<UserDTO> Map(DomainUser user)
    {
        var userDto = new UserDTO(user.Name, user.IsAdmin, user.HasBasicAuthSetup);
        return new Maybe<UserDTO>(userDto, null);
    }
}