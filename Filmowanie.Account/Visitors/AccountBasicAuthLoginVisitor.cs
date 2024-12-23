using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class AccountBasicAuthLoginVisitor : IBasicAuthLoginVisitor
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ILogger<AccountSignUpVisitor> _log;
    private readonly ILoginResultDataExtractor _extractor;

    public AccountBasicAuthLoginVisitor(IUsersQueryRepository usersQueryRepository, IHashHelper hashHelper, ILogger<AccountSignUpVisitor> log, ILoginResultDataExtractor extractor)
    {
        _usersQueryRepository = usersQueryRepository;
        _hashHelper = hashHelper;
        _log = log;
        _extractor = extractor;
    }

    public async Task<OperationResult<LoginResultData>> VisitAsync(OperationResult<BasicAuth> data, CancellationToken cancellation)
    {
        OperationResult<LoginResultData> ret;
        var basicAuth = data.Result;
        var user = await _usersQueryRepository.GetUserAsync(x => x.Email == basicAuth.Email, cancellation);

        if (user == null)
            ret = GetInvalidCredentialsError();
        else
        {
            ret = !_hashHelper.DoesHashEqual(user.PasswordHash, basicAuth.Password)
                ? GetInvalidCredentialsError()
                : _extractor.GetIdentity(user);
        }

        return ret;
    }

    private static OperationResult<LoginResultData> GetInvalidCredentialsError() => new(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue));

    public ILogger Log => _log;
}