using Filmowanie.Abstractions.Constants;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Filmowanie.Account.Services;

internal sealed class AccountSignUpService : ISignUpService
{
    private readonly IUsersCommandRepository commandRepository;
    private readonly IHttpContextWrapper httpContext;
    private readonly IHashHelper hashHelper;
    private readonly ILogger<AccountSignUpService> log;
    private readonly ILoginResultDataExtractor extractor;

    public AccountSignUpService(IUsersCommandRepository commandRepository, IHashHelper hashHelper, ILogger<AccountSignUpService> log, ILoginResultDataExtractor extractor, IHttpContextWrapper httpContext)
    {
        this.commandRepository = commandRepository;
        this.hashHelper = hashHelper;
        this.log = log;
        this.extractor = extractor;
        this.httpContext = httpContext;
    }

    public Task<Maybe<LoginResultData>> SignUp(Maybe<DomainUser> user, Maybe<BasicAuthUserData> basicAuth, CancellationToken cancellation) => user.Merge(basicAuth).AcceptAsync(SignUp, this.log, cancellation);

    public async Task<Maybe<LoginResultData>> SignUp((DomainUser, BasicAuthUserData) data, CancellationToken cancellation)
    {
        var incomingBasicAuth = data.Item2;
        var domainUser = data.Item1;

        var hash = this.hashHelper.GetHash(incomingBasicAuth.Password, domainUser.Id + domainUser.Created.Minute);
        var newAuthData = incomingBasicAuth with { Password = hash };

        try
        {
            var userEntity = await this.commandRepository.UpdatePasswordAndMail(domainUser.Id, (newAuthData.Email, newAuthData.Password), cancellation);
            var result =  this.extractor.GetIdentity(userEntity);
            await this.httpContext.SignOutAsync(Schemes.Cookie);
            return result;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error in trying to sign up");
            return new Error<LoginResultData>("No user with such id in db", ErrorType.InvalidState);
        }
    }
}