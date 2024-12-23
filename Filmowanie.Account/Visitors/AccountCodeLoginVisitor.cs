using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class AccountCodeLoginVisitor : ICodeLoginVisitor
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly ILogger<AccountSignUpVisitor> _log;
    private readonly ILoginResultDataExtractor _extractor;

    public AccountCodeLoginVisitor(IUsersQueryRepository usersQueryRepository, ILogger<AccountSignUpVisitor> log, ILoginResultDataExtractor extractor)
    {
        _usersQueryRepository = usersQueryRepository;
        _log = log;
        _extractor = extractor;
    }

    public async Task<OperationResult<LoginResultData>> VisitAsync(OperationResult<string> result, CancellationToken cancellation)
    {
        var code = result.Result;
        var user = await _usersQueryRepository.GetUserAsync(x => x.Code == code, cancellation);
        return user == null
            ? new OperationResult<LoginResultData>(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue))
            : _extractor.GetIdentity(user);
    }

    public ILogger Log => _log;
}