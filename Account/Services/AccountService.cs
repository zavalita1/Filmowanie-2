using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Abstractions;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Results;
using Filmowanie.Database.Entities;
using Filmowanie.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace Filmowanie.Account.Services;

public sealed class AccountService : IAccountService
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly IUsersCommandRepository _commandRepository;

    public AccountService(IUsersQueryRepository usersQueryRepository, IUsersCommandRepository commandRepository)
    {
        _usersQueryRepository = usersQueryRepository;
        _commandRepository = commandRepository;
    }

    public async Task<OperationResult<LoginResultData>> LoginAsync(OperationResult<string> result, CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            return OperationHelper.CancelledOperation<LoginResultData>();

        var code = result.Result;
        var user = await _usersQueryRepository.GetUserAsync(x => x.Code == code, cancellation);

        if (user == null)
            return new OperationResult<LoginResultData>(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue));

        return GetIdentity(user);
    }

    public async Task<OperationResult<LoginResultData>> LoginAsync(OperationResult<BasicAuth> data, CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            return OperationHelper.CancelledOperation<LoginResultData>();

        var basicAuth = data.Result;
        var user = await _usersQueryRepository.GetUserAsync(x => x.Email == basicAuth.Email, cancellation);

        if (user == null)
            return new OperationResult<LoginResultData>(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue));

        var passwordHashParts = user.PasswordHash.Split(";");
        var salt = passwordHashParts[^1]; // salt
        var passwordHash = passwordHashParts[0];
        var inputPasswordHash = GetPasswordHash(basicAuth.Password + salt);

        if (!string.Equals(inputPasswordHash, passwordHash))
            return GetInvalidCredentialsError();

        return GetIdentity(user);
    }

    public async Task<OperationResult<LoginResultData>> SignUpAsync(OperationResult<BasicAuth> data, CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            return OperationHelper.CancelledOperation<LoginResultData>();

        _commandRepository.UpdatePasswordAndMail()
    }

    private static OperationResult<LoginResultData> GetInvalidCredentialsError() => new(default, new Error("Invalid credentials", ErrorType.IncomingDataIssue));

    private static string GetPasswordHash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = MD5.HashData(bytes);
        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("X2"));
        }

        var hash = sb.ToString();
        return hash;
    }

    private static OperationResult<LoginResultData> GetIdentity(UserEntity user)
    {
        var hasBasicAuth = !string.IsNullOrEmpty(user.PasswordHash);
        var claims = new[]
        {
            new Claim(ClaimsTypes.UserName, user.Username),
            new Claim(ClaimsTypes.UserId, user.id),
            new Claim(ClaimsTypes.IsAdmin, user.IsAdmin.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsTypes.Code, user.Code),
            new Claim(ClaimsTypes.Tenant, user.TenantId.ToString()),
            new Claim(ClaimsTypes.HasBasicAuth, hasBasicAuth.ToString(CultureInfo.InvariantCulture)),
        };

        var claimsIdentity = new ClaimsIdentity(claims, SchemesNamesConsts.Cookie);
        var authProps = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
        };

        return new OperationResult<LoginResultData>(new LoginResultData(claimsIdentity, authProps), null);
    }
}
