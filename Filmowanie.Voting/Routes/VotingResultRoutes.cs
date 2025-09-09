using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Constants;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingResultRoutes : IVotingResultRoutes
{
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IVotingSessionMapper _sessionMapper;
    private readonly IMovieVotingResultService _movieVotingResultService;
    private readonly IVotingSessionService _votingSessionService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public VotingResultRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IVotingSessionMapper sessionMapper, IMovieVotingResultService movieVotingResultService1, ICurrentUserAccessor currentUserAccessor, IVotingSessionService votingSessionService)
    {
        _validatorAdapterProvider = validatorAdapterProvider;
        _sessionMapper = sessionMapper;
        _movieVotingResultService = movieVotingResultService1;
        _currentUserAccessor = currentUserAccessor;
        _votingSessionService = votingSessionService;
    }

    public async Task<IResult> GetResults(string votingSessionId, CancellationToken cancelToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.VotingSessionId);
        var maybeDto = validator.Validate(votingSessionId);
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(maybeDto);
        var merge = maybeDto.Merge(maybeCurrentUser);
        var maybeNullableVotingSessionId = await _sessionMapper.MapAsync(merge, cancelToken);
        var merged = maybeCurrentUser.Merge(maybeNullableVotingSessionId);
        var result = await _movieVotingResultService.GetVotingResultsAsync(merged, cancelToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionsList(CancellationToken cancelToken)
    {
        var maybeTenant = _currentUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var maybeVotingMetadata = await _movieVotingResultService.GetVotingMetadataAsync(maybeTenant, cancelToken);
        var result = _sessionMapper.Map(maybeVotingMetadata);
        
        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetWinnersList(CancellationToken cancelToken)
    {
        var maybeTenant = _currentUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var maybeVotingMetadata = await _movieVotingResultService.GetVotingMetadataAsync(maybeTenant, cancelToken);
        var merged = maybeVotingMetadata.Merge(maybeTenant);
        var maybeWinnersMetadata = await _votingSessionService.GetWinnersMetadataAsync(merged, cancelToken);
        var result = _sessionMapper.Map(maybeWinnersMetadata);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetLast10Standings(CancellationToken cancelToken)
    {
        var maybeTenant = _currentUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var result = await _votingSessionService.GetMovieVotingStandingsList(maybeTenant, cancelToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
}
}