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
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Services;

internal sealed class AccountUserService : IAccountUserService
{
    private readonly IDomainUsersRepository usersQueryRepository;
    private readonly IUsersCommandRepository usersCommandRepository;
    private readonly IGuidProvider guidProvider;
    private readonly ILogger<AccountUserService> log;
    private readonly ILoginDataExtractorAdapterFactory extractorFactory;

    public AccountUserService(IDomainUsersRepository usersQueryRepository, ILogger<AccountUserService> log, ILoginDataExtractorAdapterFactory extractorAdapterFactory, IUsersCommandRepository usersCommandRepository, IGuidProvider guidProvider)
    {
        this.usersQueryRepository = usersQueryRepository;
        this.log = log;
        this.extractorFactory = extractorAdapterFactory;
        this.usersCommandRepository = usersCommandRepository;
        this.guidProvider = guidProvider;
    }

    public Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<Code> maybeCode, CancellationToken cancelToken) => maybeCode.AcceptAsync(GetLoginDataAsync, this.log, cancelToken);

    public Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<BasicAuthUserData> maybeBasicAuthData, CancellationToken cancelToken) => maybeBasicAuthData.AcceptAsync(GetLoginDataAsync, this.log, cancelToken);

    public Task<Maybe<LoginResultData>> GetUserIdentity(Maybe<GoogleUserData> maybeGoogleUserData, CancellationToken cancelToken) => maybeGoogleUserData.AcceptAsync(GetLoginDataAsync, this.log, cancelToken);

    public Task<Maybe<IEnumerable<DomainUser>>> GetAllUsers(Maybe<VoidResult> maybe, CancellationToken cancelToken) => maybe.AcceptAsync(GetAllUsers, this.log, cancelToken);

    public Task<Maybe<DetailedUserDTO>> GetByIdAsync(Maybe<string> maybeId, CancellationToken cancelToken) => maybeId.AcceptAsync(GetByIdAsync, this.log, cancelToken);

    public Task<Maybe<VoidResult>> AddUserAsync(Maybe<DomainUser> input, CancellationToken cancelToken) => input.AcceptAsync(AddUserAsync, this.log, cancelToken);

    private async Task<Maybe<VoidResult>> AddUserAsync(DomainUser input, CancellationToken cancelToken)
    {
        if (input == default)
            return new Error<VoidResult>("Domain user is null", ErrorType.IncomingDataIssue);

        var code = this.guidProvider.NewGuid().ToString();
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

        await this.usersCommandRepository.Insert(userEntity, cancelToken);
        return new(default, null);
    }

    private async Task<Maybe<DetailedUserDTO>> GetByIdAsync(string input, CancellationToken cancelToken)
    {
        var userEntity = await this.usersQueryRepository.GetUserByIdAsync(input, cancelToken);

        if (userEntity == null)
            return new Error<DetailedUserDTO>("User not found!", ErrorType.IncomingDataIssue);

        var hasBasicAuthSetup = !string.IsNullOrEmpty(userEntity.PasswordHash);
        var outgoingDto = new DetailedUserDTO(userEntity.DisplayName, userEntity.IsAdmin, hasBasicAuthSetup, userEntity.TenantId, userEntity.Code);

        return new Maybe<DetailedUserDTO>(outgoingDto, null);
    }

    private async Task<Maybe<IEnumerable<DomainUser>>> GetAllUsers(CancellationToken cancelToken)
    {
        var allEntities = await this.usersQueryRepository.GetAllAsync(cancelToken);
        var result = allEntities.Select(x => new DomainUser(x.id, x.DisplayName, x.IsAdmin, !string.IsNullOrEmpty(x.PasswordHash), new TenantId(x.TenantId), x.Created, Enum.Parse<Gender>(x.Gender)));
        var all = result;
        return new Maybe<IEnumerable<DomainUser>>(all, null);
    }

    private async Task<Maybe<LoginResultData>> GetLoginDataAsync(Code code, CancellationToken cancellation)
    {
        var user = await this.usersQueryRepository.GetUserByCodeAsync(code.Value, cancellation);
        var extractor = this.extractorFactory.GetExtractor();
        return user == null
            ? GetInvalidCredentialsError()
            : extractor.GetIdentity(user);
    }

    private async Task<Maybe<LoginResultData>> GetLoginDataAsync<T>(T data, CancellationToken cancellation) where T : IMailBasedUserData
    {
        var user = await this.usersQueryRepository.GetUserByMailAsync(data.Email, cancellation);
        var extractor = this.extractorFactory.GetAdapter<T>();
        var result = extractor.GetLodingResultData(user, data);
        
        return result;
    }

    private static Maybe<LoginResultData> GetInvalidCredentialsError() => new Error<LoginResultData>("Invalid credentials", ErrorType.IncomingDataIssue);

    private readonly record struct User(string id, DateTime Created, int TenantId, string Email, string PasswordHash, string Code, string DisplayName, bool IsAdmin, string Gender) : IReadOnlyUserEntity;
}