using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Constants;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

// TODO UTs
internal sealed class VotingResultRoutes : IVotingResultRoutes
{
    private readonly IFluentValidatorAdapterProvider validatorAdapterProvider;
    private readonly IVotingMappersComposite mappersComposite;
    private readonly IMovieVotingResultService movieVotingResultService;
    private readonly IVotingSessionService votingSessionService;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public VotingResultRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IVotingMappersComposite mappersComposite, IMovieVotingResultService movieVotingResultService1, ICurrentUserAccessor currentUserAccessor, IVotingSessionService votingSessionService)
    {
        this.validatorAdapterProvider = validatorAdapterProvider;
        this.mappersComposite = mappersComposite;
        this.movieVotingResultService = movieVotingResultService1;
        this.currentUserAccessor = currentUserAccessor;
        this.votingSessionService = votingSessionService;
    }

    public async Task<IResult> GetResults(string votingSessionId, CancellationToken cancelToken)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<string>(KeyedServices.VotingSessionId);
        var maybeDto = validator.Validate(votingSessionId);
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(maybeDto);
        var maybeNullableVotingSessionId = await this.mappersComposite.MapAsync(maybeDto, maybeCurrentUser, cancelToken);
        var result = await this.movieVotingResultService.GetVotingResultsAsync(maybeCurrentUser, maybeNullableVotingSessionId, cancelToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetVotingSessionsList(CancellationToken cancelToken)
    {
        var maybeTenant = this.currentUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var maybeVotingMetadata = await this.movieVotingResultService.GetVotingMetadataAsync(maybeTenant, cancelToken);
        var result = this.mappersComposite.Map(maybeVotingMetadata);
        
        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetWinnersList(CancellationToken cancelToken)
    {
        var maybeTenant = this.currentUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var maybeVotingMetadata = await this.movieVotingResultService.GetVotingMetadataAsync(maybeTenant, cancelToken);
        var maybeWinnersMetadata = await this.votingSessionService.GetWinnersMetadataAsync(maybeVotingMetadata, maybeTenant, cancelToken);
        var result = this.mappersComposite.Map(maybeWinnersMetadata);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetLast10Standings(CancellationToken cancelToken)
    {
        var maybeTenant = this.currentUserAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var result = await this.votingSessionService.GetMovieVotingStandingsList(maybeTenant, cancelToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
}
}