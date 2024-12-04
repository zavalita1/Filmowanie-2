using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Constants;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingResultRoutes : IVotingResultRoutes
{
    private readonly IFluentValidatorAdapterFactory _validatorAdapterFactory;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetVotingSessionResultVisitor _getVotingSessionResultVisitor;
    private readonly IVotingSessionIdMapperVisitor _votingSessionIdMapperVisitor;
    private readonly IGetVotingSessionsMetadataVisitor _getVotingSessionsMetadataVisitor;
    private readonly IVotingSessionsMapperVisitor _mapperVisitor;

    public VotingResultRoutes(IUserIdentityVisitor userIdentityVisitor, IFluentValidatorAdapterFactory validatorAdapterFactory, IGetVotingSessionResultVisitor getVotingSessionResultVisitor, IVotingSessionIdMapperVisitor votingSessionIdMapperVisitor, IGetVotingSessionsMetadataVisitor getVotingSessionsMetadataVisitor, IVotingSessionsMapperVisitor mapperVisitor)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _validatorAdapterFactory = validatorAdapterFactory;
        _getVotingSessionResultVisitor = getVotingSessionResultVisitor;
        _votingSessionIdMapperVisitor = votingSessionIdMapperVisitor;
        _getVotingSessionsMetadataVisitor = getVotingSessionsMetadataVisitor;
        _mapperVisitor = mapperVisitor;
    }

    public async Task<IResult> GetResults(string votingSessionId, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterFactory.GetAdapter<string>(KeyedServices.VotingSessionId);
        var dtoResult = OperationResultExtensions
            .FromResult(votingSessionId)
            .Accept(validator)
            .Accept(_votingSessionIdMapperVisitor);

        var result =  await dtoResult
            .Accept(_userIdentityVisitor)
            .Pluck(x => x.Tenant)
            .Merge(dtoResult)
            .AcceptAsync(_getVotingSessionResultVisitor, cancellationToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionsList(CancellationToken cancellationToken)
    {
        var result = (await OperationResultExtensions.Empty
            .Accept(_userIdentityVisitor)
            .Pluck(x => x.Tenant)
            .AcceptAsync(_getVotingSessionsMetadataVisitor, cancellationToken))
            .Accept(_mapperVisitor);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}