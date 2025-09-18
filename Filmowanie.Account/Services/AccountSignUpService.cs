using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Services;

internal sealed class AccountSignUpService : ISignUpService
{
    private readonly IUsersCommandRepository _commandRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ILogger<AccountSignUpService> _log;
    private readonly ILoginResultDataExtractor _extractor;

    public AccountSignUpService(IUsersCommandRepository commandRepository, IHashHelper hashHelper, ILogger<AccountSignUpService> log, ILoginResultDataExtractor extractor)
    {
        _commandRepository = commandRepository;
        _hashHelper = hashHelper;
        _log = log;
        _extractor = extractor;
    }

    public Task<Maybe<LoginResultData>> SignUp(Maybe<DomainUser> user, Maybe<BasicAuth> basicAuth, CancellationToken cancellation) => user.Merge(basicAuth).AcceptAsync(SignUp, _log, cancellation);

    public async Task<Maybe<LoginResultData>> SignUp((DomainUser, BasicAuth) data, CancellationToken cancellation)
    {
        var incomingBasicAuth = data.Item2;
        var domainUser = data.Item1;

        var hash = _hashHelper.GetHash(incomingBasicAuth.Password, domainUser.Id + domainUser.Created.Minute);
        var newAuthData = incomingBasicAuth with { Password = hash };

        try
        {
            var userEntity = await _commandRepository.UpdatePasswordAndMail(domainUser.Id, newAuthData, cancellation);
            return _extractor.GetIdentity(userEntity);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in trying to sign up");
            return new Error<LoginResultData>("No user with such id in db", ErrorType.InvalidState);
        }
    }
}