using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Helpers;
using Filmowanie.Nomination.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Routes;

internal sealed class NominationRoutes : INominationRoutes
{
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetNominationsVisitor _getNominationsVisitor;
    private readonly IGetNominationsDTOVisitor _getNominationsDtoVisitor;
    private readonly IGetPostersVisitor _getPostersVisitor;
    private readonly INominationsCommandVisitor _nominationsCommandVisitor;
    private readonly IGetCurrentVotingSessionIdVisitor _currentVotingSessionIdVisitor;
    private readonly IMovieThatCanBeNominatedAgainEnricherVisitor _movieThatCanBeNominatedAgainEnricherVisitor;
    private readonly IFluentValidatorAdapterFactory _validatorAdapterFactory;

    public NominationRoutes(IUserIdentityVisitor userIdentityVisitor, IGetNominationsVisitor getNominationsVisitor, IGetCurrentVotingSessionIdVisitor currentVotingSessionIdVisitor, IMovieThatCanBeNominatedAgainEnricherVisitor movieThatCanBeNominatedAgainEnricherVisitor, IFluentValidatorAdapterFactory validatorAdapterFactory, IGetNominationsDTOVisitor getNominationsDtoVisitor, IGetPostersVisitor getPostersVisitor, INominationsCommandVisitor nominationsCommandVisitor)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _getNominationsVisitor = getNominationsVisitor;
        _currentVotingSessionIdVisitor = currentVotingSessionIdVisitor;
        _movieThatCanBeNominatedAgainEnricherVisitor = movieThatCanBeNominatedAgainEnricherVisitor;
        _validatorAdapterFactory = validatorAdapterFactory;
        _getNominationsDtoVisitor = getNominationsDtoVisitor;
        _getPostersVisitor = getPostersVisitor;
        _nominationsCommandVisitor = nominationsCommandVisitor;
    }

    public async Task<IResult> GetNominationsAsync(CancellationToken cancellationToken)
    {
        var identityResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);

        var result = (await (await identityResult
            .AcceptAsync(_currentVotingSessionIdVisitor, cancellationToken))
            .AcceptAsync(_getNominationsVisitor, cancellationToken))
            .Merge(identityResult)
            .Accept(_getNominationsDtoVisitor);

        return RoutesResultHelper.UnwrapOperationResult(result);
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

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetPosters(string movieUrl, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterFactory.GetAdapter<string>(KeyedServices.MovieUrl);
        var result = await OperationResultExtensions
            .FromResult(movieUrl)
            .Accept(validator)
            .AcceptAsync(_getPostersVisitor, cancellationToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> NominateAsync(NominationDTO dto, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterFactory.GetAdapter<(NominationDTO, DomainUser, CurrentNominationsResponse)>();
        var identityResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);
        var nominations = await (await identityResult
            .AcceptAsync(_currentVotingSessionIdVisitor, cancellationToken))
            .AcceptAsync(_getNominationsVisitor, cancellationToken);

        var result = await OperationResultExtensions
            .FromResult(dto)
            .Merge(identityResult)
            .Merge(nominations)
            .Flatten()
            .Accept(validator)
            .AcceptAsync(_nominationsCommandVisitor, cancellationToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}