using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class AccountSignUpVisitor : ISignUpVisitor
{
    private readonly IUsersCommandRepository _commandRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ILogger<AccountSignUpVisitor> _log;
    private readonly ILoginResultDataExtractor _extractor;

    public AccountSignUpVisitor(IUsersCommandRepository commandRepository, IHashHelper hashHelper, ILogger<AccountSignUpVisitor> log, ILoginResultDataExtractor extractor)
    {
        _commandRepository = commandRepository;
        _hashHelper = hashHelper;
        _log = log;
        _extractor = extractor;
    }

    public async Task<OperationResult<LoginResultData>> VisitAsync(OperationResult<(DomainUser, BasicAuth)> data, CancellationToken cancellation)
    {
        var incomingBasicAuth = data.Result.Item2;
        var domainUser = data.Result.Item1;

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
            return new OperationResult<LoginResultData>(default, new Error("No user with such id in db", ErrorType.InvalidState));
        }
    }

    public ILogger Log => _log;
}