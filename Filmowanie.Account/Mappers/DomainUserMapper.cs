using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Mappers;

internal sealed class DomainUserMapper : IDomainUserMapper
{
    private readonly ILogger<DomainUserMapper> _log;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;

    public DomainUserMapper(
        ILogger<DomainUserMapper> log,
        IDateTimeProvider dateTimeProvider,
        IGuidProvider guidProvider)
    {
        _log = log;
        _dateTimeProvider = dateTimeProvider;
        _guidProvider = guidProvider;
    }

    public Maybe<DomainUser> Map(Maybe<UserDTO> maybeDto, Maybe<DomainUser> maybeUser) => maybeDto.Merge(maybeUser).Accept(MapToDomainInternal, _log);

    private Maybe<DomainUser> MapToDomainInternal((UserDTO, DomainUser CurrentUser) input)
    {
        var now = _dateTimeProvider.Now;
        var guid = _guidProvider.NewGuid();
        var userId = $"user-{guid}";
        var domainUser = new DomainUser(userId, input.Item1.Id, false, false, input.CurrentUser.Tenant, now, input.Item2.Gender);
        return new Maybe<DomainUser>(domainUser, null);
    }
}
