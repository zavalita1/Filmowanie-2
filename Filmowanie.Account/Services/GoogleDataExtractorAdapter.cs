using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Account.Services;

internal sealed class GoogleDataExtractorAdapter : ILoginDataExtractorAdapter<GoogleUserData>
{
    private readonly ILoginResultDataExtractorDecorator loginResultDataExtractor;

    public GoogleDataExtractorAdapter(ILoginResultDataExtractorDecorator loginResultDataExtractor)
    {
        this.loginResultDataExtractor = loginResultDataExtractor;
    }

    public Maybe<LoginResultData> GetLodingResultData(IReadOnlyUserEntity? dbUser, GoogleUserData mailBasedUserData)
    {
        if (dbUser == null)
            return GetInvalidCredentialsError();

        return this.loginResultDataExtractor.GetIdentity(dbUser, mailBasedUserData.AccessToken, mailBasedUserData.RefreshToken);
    }

    private static Maybe<LoginResultData> GetInvalidCredentialsError() => new Error<LoginResultData>("Jeżeli jeszcze nie logowaliście się towarzyszu tą metodą, musicie poprosić Cepigę, żeby to dla was ustawił. Jeżeli widzicie ten błąd, mimo że kiedyś się logowaliście - coś się zesrao", ErrorType.IncomingDataIssue);
}
