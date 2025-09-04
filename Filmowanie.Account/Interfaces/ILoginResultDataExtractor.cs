using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Results;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Account.Interfaces;

internal interface ILoginResultDataExtractor
{
    Maybe<LoginResultData> GetIdentity(IReadOnlyUserEntity user);
}