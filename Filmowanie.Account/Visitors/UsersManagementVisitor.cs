using Filmowanie.Abstractions;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;

namespace Filmowanie.Account.Visitors;

internal class UsersManagementVisitor : IGetAllUsersVisitor, IAddUserVisitor
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly IUsersCommandRepository _usersCommandRepository;

    public UsersManagementVisitor(IUsersQueryRepository usersQueryRepository, IUsersCommandRepository usersCommandRepository)
    {
        _usersQueryRepository = usersQueryRepository;
        _usersCommandRepository = usersCommandRepository;
    }

    private async Task<IEnumerable<DomainUser>> GetAll(CancellationToken cancellation)
    {
        var allEntities = await _usersQueryRepository.GetAllAsync(cancellation);
        var result = allEntities.Select(x => new DomainUser(x.Id, x.Username, x.IsAdmin, !string.IsNullOrEmpty(x.PasswordHash), x.TenantId, x.Created));
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
        var code = Guid.NewGuid().ToString();
        var userEntity = new User
        {
            Code = code,
            Created = domainUser.Created, 
            Email = null!, 
            Id = domainUser.Id,
            IsAdmin = domainUser.IsAdmin,
            PasswordHash = null!,
            TenantId = domainUser.TenantId,
            Username = domainUser.Username
        };

        await _usersCommandRepository.Insert(userEntity, cancellationToken);
        return new(default, null);
    }

    private class User : IReadOnlyUserEntity
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Code { get; set; }
        public string Username { get; set; }
        public int TenantId { get; set; }
        public bool IsAdmin { get; set; }
    }
}