using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Mappers;

internal sealed class DomainUserMapper : IDomainUserMapper
{
    private readonly ILogger<DomainUserMapper> log;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IGuidProvider guidProvider;

    public DomainUserMapper(
        ILogger<DomainUserMapper> log,
        IDateTimeProvider dateTimeProvider,
        IGuidProvider guidProvider)
    {
        this.log = log;
        this.dateTimeProvider = dateTimeProvider;
        this.guidProvider = guidProvider;
    }

    public Maybe<DomainUser> Map(Maybe<UserDTO> maybeDto, Maybe<DomainUser> maybeUser) => maybeDto.Merge(maybeUser).Accept(MapToDomainInternal, log);

    private Maybe<DomainUser> MapToDomainInternal((UserDTO, DomainUser CurrentUser) input)
    {
        var now = dateTimeProvider.Now;
        var guid = guidProvider.NewGuid();
        var userId = $"user-{guid}";
        var domainUser = new DomainUser(userId, input.Item1.Id, false, false, input.CurrentUser.Tenant, now, input.Item2.Gender);
        return new Maybe<DomainUser>(domainUser, null);
    }
}
