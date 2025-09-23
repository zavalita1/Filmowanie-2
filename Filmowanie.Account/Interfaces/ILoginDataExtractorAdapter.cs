using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Account.Interfaces;

internal interface ILoginDataExtractorAdapter<T> where T : IMailBasedUserData
{
    public Maybe<LoginResultData> GetLoginResultData(IReadOnlyUserEntity? dbUser, T mailBasedUserData);
}
