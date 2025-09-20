using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Account.Services;

internal sealed class BasicAuthDataExtractorAdapter : ILoginDataExtractorAdapter<BasicAuthUserData>
{
    private readonly IHashHelper hashHelper;
    private readonly ILoginResultDataExtractor loginResultDataExtractor;


    public BasicAuthDataExtractorAdapter(IHashHelper hashHelper, ILoginResultDataExtractor loginResultDataExtractor)
    {
        this.hashHelper = hashHelper;
        this.loginResultDataExtractor = loginResultDataExtractor;
    }

    public Maybe<LoginResultData> GetLodingResultData(IReadOnlyUserEntity? dbUser, BasicAuthUserData mailBasedUserData)
    {
        if (dbUser == null)
            return GetInvalidCredentialsError();

        else
        {
            return !this.hashHelper.DoesHashEqual(dbUser.PasswordHash, mailBasedUserData.Password)
                ? GetInvalidCredentialsError()
                : this.loginResultDataExtractor.GetIdentity(dbUser);
        }
    }
    
    private static Maybe<LoginResultData> GetInvalidCredentialsError() => new Error<LoginResultData>("Invalid credentials", ErrorType.IncomingDataIssue);
}