using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Filmowanie.Database.Interfaces;

namespace Filmowanie.Account.Visitors;

internal sealed class UserDTOMapperVisitor : IUserMapperVisitor, IEnrichUserVisitor, IUserReverseMapperVisitor
{
    private readonly IUsersQueryRepository _usersRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserDTOMapperVisitor(IUsersQueryRepository usersRepository, IDateTimeProvider dateTimeProvider)
    {
        _usersRepository = usersRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public OperationResult<UserDTO> Visit(OperationResult<DomainUser> user)
    {
        var userDto = new UserDTO(user.Result.Username, user.Result.IsAdmin, user.Result.HasBasicAuthSetup);
        return new OperationResult<UserDTO>(userDto, null);
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

    public OperationResult<DomainUser> Visit(OperationResult<(DTOs.Incoming.UserDTO, DomainUser CurrentUser)> input)
    {
        var now = _dateTimeProvider.Now;
        var domainUser = new DomainUser(input.Result!.Item1.Id, input.Result.Item1.DisplayName, false, false, input.Result.CurrentUser.TenantId, now);
        return new OperationResult<DomainUser>(domainUser, null);
    }
}