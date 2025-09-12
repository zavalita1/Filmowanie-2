using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Services;

internal sealed class AccountUserService : IAccountUserService
{
    private readonly IDomainUsersRepository _usersQueryRepository;
    private readonly IUsersCommandRepository _usersCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<AccountUserService> _log;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly IHashHelper _hashHelper;

    public AccountUserService(IDomainUsersRepository usersQueryRepository, ILogger<AccountUserService> log, ILoginResultDataExtractor extractor, IHashHelper hashHelper, IUsersCommandRepository usersCommandRepository, IGuidProvider guidProvider)
    {
        _usersQueryRepository = usersQueryRepository;
        _log = log;
        _extractor = extractor;
        _hashHelper = hashHelper;
        _usersCommandRepository = usersCommandRepository;
        _guidProvider = guidProvider;
    }

    public Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<string> maybeCode, CancellationToken cancelToken) => maybeCode.AcceptAsync(GetLoginDataAsync, _log, cancelToken);

    public Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<BasicAuth> maybeBasicAuthData, CancellationToken cancelToken) => maybeBasicAuthData.AcceptAsync(GetLoginDataAsync, _log, cancelToken);

    public Task<Maybe<IEnumerable<DomainUser>>> GetAllUsers(Maybe<VoidResult> maybe, CancellationToken cancelToken) => maybe.AcceptAsync(GetAllUsers, _log, cancelToken);

    public Task<Maybe<DetailedUserDTO>> GetByIdAsync(Maybe<string> maybeId, CancellationToken cancelToken) => maybeId.AcceptAsync(GetByIdAsync, _log, cancelToken);

    public Task<Maybe<VoidResult>> AddUserAsync(Maybe<DomainUser> input, CancellationToken cancelToken) => input.AcceptAsync(AddUserAsync, _log, cancelToken);

    private async Task<Maybe<VoidResult>> AddUserAsync(DomainUser input, CancellationToken cancelToken)
    {
        if (input == default)
            return new Error<VoidResult>("Domain user is null", ErrorType.IncomingDataIssue);

        var code = _guidProvider.NewGuid().ToString();
        var userEntity = new User
        {
            Code = code,
            Created = input.Created,
            Email = null!,
            id = input.Id,
            IsAdmin = input.IsAdmin,
            PasswordHash = null!,
            TenantId = input.Tenant.Id,
            DisplayName = input.Name // TODO fix this
        };

        await _usersCommandRepository.Insert(userEntity, cancelToken);
        return new(default, null);
    }

    private async Task<Maybe<DetailedUserDTO>> GetByIdAsync(string input, CancellationToken cancelToken)
    {
        var userEntity = await _usersQueryRepository.GetUserByIdAsync(input, cancelToken);

        if (userEntity == null)
            return new Error<DetailedUserDTO>("User not found!", ErrorType.IncomingDataIssue);

        var hasBasicAuthSetup = !string.IsNullOrEmpty(userEntity.PasswordHash);
        var outgoingDto = new DetailedUserDTO(userEntity.DisplayName, userEntity.IsAdmin, hasBasicAuthSetup, userEntity.TenantId, userEntity.Code);

        return new Maybe<DetailedUserDTO>(outgoingDto, null);
    }

    private async Task<Maybe<IEnumerable<DomainUser>>> GetAllUsers(CancellationToken cancelToken)
    {
        var allEntities = await _usersQueryRepository.GetAllAsync(cancelToken);
        var result = allEntities.Select(x => new DomainUser(x.id, x.DisplayName, x.IsAdmin, !string.IsNullOrEmpty(x.PasswordHash), new TenantId(x.TenantId), x.Created));
        var all = result;
        return new Maybe<IEnumerable<DomainUser>>(all, null);
    }

    private async Task<Maybe<LoginResultData>> GetLoginDataAsync(string code, CancellationToken cancellation)
    {
        var user = await _usersQueryRepository.GetUserByCodeAsync(code, cancellation);
        return user == null
            ? GetInvalidCredentialsError()
            : _extractor.GetIdentity(user);
    }

    private async Task<Maybe<LoginResultData>> GetLoginDataAsync(BasicAuth data, CancellationToken cancellation)
    {
        Maybe<LoginResultData> ret;
        var user = await _usersQueryRepository.GetUserByMailAsync(data.Email, cancellation);

        if (user == null)
            ret = GetInvalidCredentialsError();
        else
        {
            ret = !_hashHelper.DoesHashEqual(user.PasswordHash, data.Password)
                ? GetInvalidCredentialsError()
                : _extractor.GetIdentity(user);
        }

        return ret;
    }

    private static Maybe<LoginResultData> GetInvalidCredentialsError() => new Error<LoginResultData>("Invalid credentials", ErrorType.IncomingDataIssue);

    private readonly record struct User(string id, DateTime Created, int TenantId, string Email, string PasswordHash, string Code, string DisplayName, bool IsAdmin) : IReadOnlyUserEntity;
}