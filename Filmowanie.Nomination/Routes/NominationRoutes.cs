using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Routes;

internal sealed class NominationRoutes : INominationRoutes
{
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetNominationsVisitor _getNominationsVisitor;
    private readonly IGetNominationsDTOVisitor _getNominationsDtoVisitor;
    private readonly IGetPostersVisitor _getPostersVisitor;
    private readonly INominationsCompleterVisitor _nominationsCompleterVisitor;
    private readonly INominationsResetterVisitor _nominationsResetterVisitor;
    private readonly IGetCurrentVotingSessionIdVisitor _currentVotingSessionIdVisitor;
    private readonly IRequireCurrentVotingSessionIdVisitor _requireCurrentVotingSessionIdVisitor;
    private readonly IMovieThatCanBeNominatedAgainEnricherVisitor _movieThatCanBeNominatedAgainEnricherVisitor;
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IRoutesResultHelper _routesResultHelper;

    public NominationRoutes(IUserIdentityVisitor userIdentityVisitor, IGetNominationsVisitor getNominationsVisitor, IGetCurrentVotingSessionIdVisitor currentVotingSessionIdVisitor, IMovieThatCanBeNominatedAgainEnricherVisitor movieThatCanBeNominatedAgainEnricherVisitor, IFluentValidatorAdapterProvider validatorAdapterProvider, IGetNominationsDTOVisitor getNominationsDtoVisitor, IGetPostersVisitor getPostersVisitor, INominationsCompleterVisitor nominationsCompleterVisitor, INominationsResetterVisitor nominationsResetterVisitor, IRequireCurrentVotingSessionIdVisitor requireCurrentVotingSessionIdVisitor, IRoutesResultHelper routesResultHelper)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _getNominationsVisitor = getNominationsVisitor;
        _currentVotingSessionIdVisitor = currentVotingSessionIdVisitor;
        _movieThatCanBeNominatedAgainEnricherVisitor = movieThatCanBeNominatedAgainEnricherVisitor;
        _validatorAdapterProvider = validatorAdapterProvider;
        _getNominationsDtoVisitor = getNominationsDtoVisitor;
        _getPostersVisitor = getPostersVisitor;
        _nominationsCompleterVisitor = nominationsCompleterVisitor;
        _nominationsResetterVisitor = nominationsResetterVisitor;
        _requireCurrentVotingSessionIdVisitor = requireCurrentVotingSessionIdVisitor;
        _routesResultHelper = routesResultHelper;
    }

    public async Task<IResult> GetNominationsAsync(CancellationToken cancellationToken)
    {
        var identityResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);

        var result = (await (await identityResult
            .AcceptAsync(_currentVotingSessionIdVisitor, cancellationToken))
            .AcceptAsync(_getNominationsVisitor, cancellationToken))
            .Merge(identityResult)
            .Accept(_getNominationsDtoVisitor);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetNominationsFullDataAsync(CancellationToken cancellationToken)
    {
        var identityResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);

        var result = await (await (await identityResult
                        .AcceptAsync(_currentVotingSessionIdVisitor, cancellationToken))
                        .AcceptAsync(_getNominationsVisitor, cancellationToken))
                        .Merge(identityResult)
                        .Accept(_getNominationsDtoVisitor)
                        .Merge(identityResult)
                        .AcceptAsync(_movieThatCanBeNominatedAgainEnricherVisitor, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetPostersAsync(string movieUrl, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieUrl);
        var result = await movieUrl
            .ToOperationResult()
            .Accept(validator)
            .AcceptAsync(_getPostersVisitor, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> DeleteMovieAsync(string movieId, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieId);
        var identityResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);
        var votingSessionIdResult = (await identityResult
                .AcceptAsync(_currentVotingSessionIdVisitor, cancellationToken))
                .Accept(_requireCurrentVotingSessionIdVisitor);

        var result = await movieId
            .ToOperationResult()
            .Accept(validator)
            .Merge(identityResult)
            .Merge(votingSessionIdResult)
            .Flatten()
            .AcceptAsync(_nominationsResetterVisitor, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> NominateAsync(NominationDTO dto, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<(NominationDTO, DomainUser, CurrentNominationsResponse)>();
        var identityResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);
        var nominations = await (await identityResult
            .AcceptAsync(_currentVotingSessionIdVisitor, cancellationToken))
            .AcceptAsync(_getNominationsVisitor, cancellationToken);

        var result = await dto
            .ToOperationResult()
            .Merge(identityResult)
            .Merge(nominations)
            .Flatten()
            .Accept(validator)
            .AcceptAsync(_nominationsCompleterVisitor, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }
}