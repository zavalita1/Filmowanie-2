using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class AddUserVisitor : IAddUserVisitor
{
    private readonly IUsersCommandRepository _usersCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<AddUserVisitor> _log;

    public AddUserVisitor(IUsersCommandRepository usersCommandRepository, IGuidProvider guidProvider, ILogger<AddUserVisitor> log)
    {
        _usersCommandRepository = usersCommandRepository;
        _guidProvider = guidProvider;
        _log = log;
    }

    public async Task<OperationResult<object>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        if (input.Result == default)
            return new OperationResult<object>(null, new Error("Domain user is null", ErrorType.IncomingDataIssue));

        var domainUser = input.Result;
        var code = _guidProvider.NewGuid().ToString();
        var userEntity = new User
        {
            Code = code,
            Created = domainUser.Created, 
            Email = null!, 
            id = domainUser.Id,
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
        public string id { get; set; }
        public DateTime Created { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public int TenantId { get; set; }
        public bool IsAdmin { get; set; }
    }
}