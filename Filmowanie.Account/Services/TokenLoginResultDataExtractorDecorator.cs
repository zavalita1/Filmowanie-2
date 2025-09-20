using System.Security.Claims;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Helpers;

internal sealed class TokenLoginResultDataExtractorDecorator : ILoginResultDataExtractorDecorator
{
    private readonly ILoginResultDataExtractor decorated;
    private readonly ILogger<TokenLoginResultDataExtractorDecorator> log;

    public TokenLoginResultDataExtractorDecorator(ILoginResultDataExtractor decorated, ILogger<TokenLoginResultDataExtractorDecorator> log)
    {
        this.decorated = decorated;
        this.log = log;
    }

    public Maybe<LoginResultData> GetIdentity(IReadOnlyUserEntity user, string accessToken, string refreshToken)
    {
        var result = this.decorated.GetIdentity(user);
        return result.Accept(Decorate, this.log);

        Maybe<LoginResultData> Decorate(LoginResultData loginResultData)
        {
            loginResultData.Identity.AddClaim(new Claim(ClaimsTypes.AccessToken, accessToken));
            loginResultData.Identity.AddClaim(new Claim(ClaimsTypes.RefreshToken, refreshToken));
            return loginResultData.AsMaybe();
        }
    }
}