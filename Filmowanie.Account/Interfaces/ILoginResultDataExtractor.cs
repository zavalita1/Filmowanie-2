using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Account.Interfaces;

internal interface ILoginResultDataExtractor
{
    Maybe<LoginResultData> GetIdentity(IReadOnlyUserEntity user);
}
