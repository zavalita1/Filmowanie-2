using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class GetAllUsersVisitor : IGetAllUsersVisitor
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly ILogger<GetAllUsersVisitor> _log;

    public GetAllUsersVisitor(IUsersQueryRepository usersQueryRepository, ILogger<GetAllUsersVisitor> log)
    {
        _usersQueryRepository = usersQueryRepository;
        _log = log;
    }

    public async Task<OperationResult<IEnumerable<DomainUser>>> VisitAsync<T>(OperationResult<T> input, CancellationToken cancellationToken)
    {
        var allEntities = await _usersQueryRepository.GetAllAsync(cancellationToken);
        var result = allEntities.Select(x => new DomainUser(x.id, x.DisplayName, x.IsAdmin, !string.IsNullOrEmpty(x.PasswordHash), new TenantId(x.TenantId), x.Created));
        var all = result;
        return new OperationResult<IEnumerable<DomainUser>>(all, null);
    }

    public ILogger Log => _log;
}