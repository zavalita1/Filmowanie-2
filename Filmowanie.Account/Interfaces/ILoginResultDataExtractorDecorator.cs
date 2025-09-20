using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Account.Interfaces;

internal interface ILoginResultDataExtractorDecorator
{
    Maybe<LoginResultData> GetIdentity(IReadOnlyUserEntity user, string accessToken, string refreshToken);
}
