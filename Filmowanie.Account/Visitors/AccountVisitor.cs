using System.Globalization;
using System.Security.Claims;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

public sealed class AccountVisitor : ICodeLoginVisitor, IBasicAuthLoginVisitor, ISignUpVisitor
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly IUsersCommandRepository _commandRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ILogger<AccountVisitor> _log;

    public AccountVisitor(IUsersQueryRepository usersQueryRepository, IUsersCommandRepository commandRepository, IHashHelper hashHelper, ILogger<AccountVisitor> log)
    {
        _usersQueryRepository = usersQueryRepository;
        _commandRepository = commandRepository;
        _hashHelper = hashHelper;
        _log = log;
    }

    public async Task<OperationResult<LoginResultData>> VisitAsync(OperationResult<string> result, CancellationToken cancellation) => await LoginWithCodeAsync(result, cancellation);

    public async Task<OperationResult<LoginResultData>> VisitAsync(OperationResult<BasicAuth> data, CancellationToken cancellation) => await LoginWithBasicAuthAsync(data, cancellation);

    public async Task<OperationResult<LoginResultData>> VisitAsync(OperationResult<(DomainUser, BasicAuth)> data, CancellationToken cancellation)
    {
        var incomingBasicAuth = data.Result.Item2;
        var domainUser = data.Result.Item1;

        var hash = _hashHelper.GetHash(incomingBasicAuth.Password, domainUser.Id + domainUser.Created.Minute);
        var newAuthData = incomingBasicAuth with { Password = hash };

        try
        {
            var userEntity = await _commandRepository.UpdatePasswordAndMail(domainUser.Id, newAuthData, cancellation);
            return GetIdentity(userEntity);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in trying to sign up");
            return new OperationResult<LoginResultData>(default, new Error("No user with such id in db", ErrorType.InvalidState));
        }
    }

    private async Task<OperationResult<LoginResultData>> LoginWithCodeAsync(OperationResult<string> result, CancellationToken cancellation)
    {
        var code = result.Result;
        var user = await _usersQueryRepository.GetUserAsync(x => x.Code == code, cancellation);

        return user == null 
            ? new OperationResult<LoginResultData>(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue)) 
            : GetIdentity(user);
    }

    private async Task<OperationResult<LoginResultData>> LoginWithBasicAuthAsync(OperationResult<BasicAuth> data, CancellationToken cancellation)
    {
        var basicAuth = data.Result;
        var user = await _usersQueryRepository.GetUserAsync(x => x.Email == basicAuth.Email, cancellation);

        if (user == null)
            return new OperationResult<LoginResultData>(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue));

        return !_hashHelper.DoesHashEqual(user.PasswordHash, basicAuth.Password) 
            ? GetInvalidCredentialsError() 
            : GetIdentity(user);
    }

    private static OperationResult<LoginResultData> GetInvalidCredentialsError() => new(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue));

  
    private static OperationResult<LoginResultData> GetIdentity(IReadOnlyUserEntity user)
    {
        var hasBasicAuth = !string.IsNullOrEmpty(user.PasswordHash);
        var claims = new[]
        {
            new Claim(ClaimsTypes.UserName, user.Username),
            new Claim(ClaimsTypes.UserId, user.Id),
            new Claim(ClaimsTypes.IsAdmin, user.IsAdmin.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsTypes.Tenant, user.TenantId.ToString()),
            new Claim(ClaimsTypes.HasBasicAuth, hasBasicAuth.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsTypes.Created, user.Created.ToString("O")),
        };

        var claimsIdentity = new ClaimsIdentity(claims, Schemes.Cookie);
        var authProps = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(Authorization.AuthorizationExpirationTimeInDays),
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
        };

        return new OperationResult<LoginResultData>(new LoginResultData(claimsIdentity, authProps), null);
    }
}