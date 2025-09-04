using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Voting.Constants;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingResultRoutes : IVotingResultRoutes
{
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IVotingSessionMapper _sessionMapper;
    private readonly ICurrentVotingSessionIdAccessor _votingSessionIdAccessor;
    private readonly IMovieVotingSessionService _movieVotingSessionService;
    private readonly IVotingSessionService _votingSessionService;
    private readonly IDomainUserAccessor _domainUserAccessor;

    public VotingResultRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IVotingSessionMapper sessionMapper, ICurrentVotingSessionIdAccessor votingSessionIdAccessor, IMovieVotingSessionService movieVotingSessionService1, IDomainUserAccessor domainUserAccessor, IVotingSessionService votingSessionService)
    {
        _validatorAdapterProvider = validatorAdapterProvider;
        _sessionMapper = sessionMapper;
        _votingSessionIdAccessor = votingSessionIdAccessor;
        _movieVotingSessionService = movieVotingSessionService1;
        _domainUserAccessor = domainUserAccessor;
        _votingSessionService = votingSessionService;
    }

    public async Task<IResult> GetResults(string votingSessionId, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.VotingSessionId);
        var maybeDto = validator.Validate(votingSessionId);
        var maybeNullableVotingSessionId = _sessionMapper.Map(maybeDto);
        var maybeVotingSessionId = _votingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableVotingSessionId);
        var maybeTenant = _domainUserAccessor.GetDomainUser(maybeVotingSessionId.AsVoid()).Map(x => x.Tenant);
        var merged = maybeTenant.Merge(maybeNullableVotingSessionId);
        var result = await _movieVotingSessionService.GetVotingResultsAsync(merged, cancellationToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionsList(CancellationToken cancellationToken)
    {
        var maybeTenant = _domainUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var maybeVotingMetadata = await _movieVotingSessionService.GetVotingMetadataAsync(maybeTenant, cancellationToken);
        var result = _sessionMapper.Map(maybeVotingMetadata);
        
        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetWinnersList(CancellationToken cancellationToken)
    {
        var maybeTenant = _domainUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var maybeVotingMetadata = await _movieVotingSessionService.GetVotingMetadataAsync(maybeTenant, cancellationToken);
        var merged = maybeVotingMetadata.Merge(maybeTenant);
        var maybeWinnersMetadata = await _votingSessionService.GetWinnersMetadataAsync(merged, cancellationToken);
        var result = _sessionMapper.Map(maybeWinnersMetadata);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetLast10Standings(CancellationToken cancellationToken)
    {
        var maybeTenant = _domainUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var result = await _votingSessionService.GetMovieVotingStandingsList(maybeTenant, cancellationToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
}
}