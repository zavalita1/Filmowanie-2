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
    private readonly IGetVotingResultDTOVisitor _getVotingSessionResultVisitor;
    private readonly IVotingSessionIdMapperVisitor _votingSessionIdMapperVisitor;
    private readonly IGetVotingSessionsMetadataVisitor _getVotingSessionsMetadataVisitor;
    private readonly IWinnersMetadataMapperVisitor _winnersMetadataMapperVisitor;
    private readonly IHistoryDTOMapperVisitor _historyDtoMapperVisitor;
    private readonly IHistoryStandingsDTOMapperVisitor _historyStandingsDtoMapperVisitor;
    private readonly IVotingSessionsMapperVisitor _mapperVisitor;

    public VotingResultRoutes(IUserIdentityVisitor userIdentityVisitor, IFluentValidatorAdapterFactory validatorAdapterFactory, IGetVotingResultDTOVisitor getVotingSessionResultVisitor, IVotingSessionIdMapperVisitor votingSessionIdMapperVisitor, IGetVotingSessionsMetadataVisitor getVotingSessionsMetadataVisitor, IVotingSessionsMapperVisitor mapperVisitor, IWinnersMetadataMapperVisitor winnersMetadataMapperVisitor, IHistoryDTOMapperVisitor historyDtoMapperVisitor, IHistoryStandingsDTOMapperVisitor historyStandingsDtoMapperVisitor)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _validatorAdapterFactory = validatorAdapterFactory;
        _getVotingSessionResultVisitor = getVotingSessionResultVisitor;
        _votingSessionIdMapperVisitor = votingSessionIdMapperVisitor;
        _getVotingSessionsMetadataVisitor = getVotingSessionsMetadataVisitor;
        _mapperVisitor = mapperVisitor;
        _winnersMetadataMapperVisitor = winnersMetadataMapperVisitor;
        _historyDtoMapperVisitor = historyDtoMapperVisitor;
        _historyStandingsDtoMapperVisitor = historyStandingsDtoMapperVisitor;
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

    public async Task<IResult> GetWinnersList(CancellationToken cancellationToken)
    {
        var tenantResult = OperationResultExtensions.Empty
            .Accept(_userIdentityVisitor)
            .Pluck(x => x.Tenant);

        var result = (await (await tenantResult
                .AcceptAsync(_getVotingSessionsMetadataVisitor, cancellationToken))
            .Merge(tenantResult)
            .AcceptAsync(_winnersMetadataMapperVisitor, cancellationToken))
            .Accept(_historyDtoMapperVisitor);
            
        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetLast10Standings(CancellationToken cancellationToken)
    {
        var result = (await OperationResultExtensions.Empty
            .Accept(_userIdentityVisitor)
            .Pluck(x => x.Tenant)
            .AcceptAsync(_historyStandingsDtoMapperVisitor, cancellationToken));

        return RoutesResultHelper.UnwrapOperationResult(result);
}
}