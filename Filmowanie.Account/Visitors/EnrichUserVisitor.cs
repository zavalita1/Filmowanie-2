using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class EnrichUserVisitor : IEnrichUserVisitor
{
    private readonly IUsersQueryRepository _usersRepository;
    private readonly ILogger<EnrichUserVisitor> _log;

    public EnrichUserVisitor(IUsersQueryRepository usersRepository, ILogger<EnrichUserVisitor> log)
    {
        _usersRepository = usersRepository;
        _log = log;
    }

    public async Task<OperationResult<DetailedUserDTO>> VisitAsync(OperationResult<string> input, CancellationToken cancellationToken)
    {
        var userEntity = await _usersRepository.GetUserAsync(x => x.id == input.Result!, cancellationToken);

        if (userEntity == null)
            return new OperationResult<DetailedUserDTO>(default, new Error("User not found!", ErrorType.IncomingDataIssue));

        var hasBasicAuthSetup = !string.IsNullOrEmpty(userEntity.PasswordHash);
        var outgoingDto = new DetailedUserDTO(userEntity.DisplayName, userEntity.IsAdmin, hasBasicAuthSetup, userEntity.TenantId, userEntity.Code);

        return new OperationResult<DetailedUserDTO>(outgoingDto, null);
    }

    public ILogger Log => _log;
}