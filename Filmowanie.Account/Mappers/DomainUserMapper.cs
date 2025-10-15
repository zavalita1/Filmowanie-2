using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
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

    public Maybe<DomainUser> Map(Maybe<CreateUserDTO> maybeDto, Maybe<DomainUser> maybeUser) => maybeDto.Merge(maybeUser).Accept(MapToDomainInternal, log);

    private Maybe<DomainUser> MapToDomainInternal((CreateUserDTO, DomainUser CurrentUser) input)
    {
        var now = dateTimeProvider.Now;
        var guid = guidProvider.NewGuid();
        var userId = $"user-{guid}";
        if (!Enum.TryParse<Gender>(input.Item1.Gender, out var gender))
            gender = Gender.Unspecified;

        var domainUser = new DomainUser(userId, input.Item1.Username, false, false, input.CurrentUser.Tenant, now, gender);
        return new Maybe<DomainUser>(domainUser, null);
    }
}
