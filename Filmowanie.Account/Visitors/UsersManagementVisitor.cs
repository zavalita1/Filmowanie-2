using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal class UsersManagementVisitor : IGetAllUsersVisitor, IAddUserVisitor
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly IUsersCommandRepository _usersCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<UsersManagementVisitor> _log;

    public UsersManagementVisitor(IUsersQueryRepository usersQueryRepository, IUsersCommandRepository usersCommandRepository, IGuidProvider guidProvider, ILogger<UsersManagementVisitor> log)
    {
        _usersQueryRepository = usersQueryRepository;
        _usersCommandRepository = usersCommandRepository;
        _guidProvider = guidProvider;
        _log = log;
    }

    private async Task<IEnumerable<DomainUser>> GetAll(CancellationToken cancellation)
    {
        var allEntities = await _usersQueryRepository.GetAllAsync(cancellation);
        var result = allEntities.Select(x => new DomainUser(x.Id, x.DisplayName, x.IsAdmin, !string.IsNullOrEmpty(x.PasswordHash), new TenantId(x.TenantId), x.Created));
        return result;
    }

    public async Task<OperationResult<IEnumerable<DomainUser>>> VisitAsync<T>(OperationResult<T> input, CancellationToken cancellationToken)
    {
        var all = await GetAll(cancellationToken);
        return new OperationResult<IEnumerable<DomainUser>>(all, null);
    }

    public async Task<OperationResult<object>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var domainUser = input.Result!;
        var code = _guidProvider.NewGuid().ToString();
        var userEntity = new User
        {
            Code = code,
            Created = domainUser.Created, 
            Email = null!, 
            Id = domainUser.Id,
            IsAdmin = domainUser.IsAdmin,
            PasswordHash = null!,
            TenantId = domainUser.Tenant.Id,
            DisplayName = domainUser.Name // TODO fix this
        };

        await _usersCommandRepository.Insert(userEntity, cancellationToken);
        return new(default, null);
    }

    public ILogger Log => _log;

    private class User : IReadOnlyUserEntity
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public int TenantId { get; set; }
        public bool IsAdmin { get; set; }
    }
}