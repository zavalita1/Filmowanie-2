using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Routes;

internal sealed class NominationRoutes : INominationRoutes
{
    private readonly IDomainUserAccessor _domainUserAccessor;
    private readonly ICurrentVotingSessionIdAccessor _currentVotingSessionIdAccessor;
    private readonly INominationsService _nominationsService;
    private readonly INominationsMapper _nominationsMapper;
    private readonly IMoviePostersService _moviePostersService;

    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IRoutesResultHelper _routesResultHelper;

    public NominationRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IMoviePostersService moviePostersService, IRoutesResultHelper routesResultHelper, IDomainUserAccessor domainUserAccessor, ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor, INominationsService nominationsService, INominationsMapper nominationsMapper)
    {
        _validatorAdapterProvider = validatorAdapterProvider;
        _moviePostersService = moviePostersService;
        _routesResultHelper = routesResultHelper;
        _domainUserAccessor = domainUserAccessor;
        _currentVotingSessionIdAccessor = currentVotingSessionIdAccessor;
        _nominationsService = nominationsService;
        _nominationsMapper = nominationsMapper;
    }

    public async Task<IResult> GetNominationsAsync(CancellationToken cancellationToken)
    {
        var maybeCurrentUser = _domainUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancellationToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancellationToken);
        var merged = maybeNominations.Merge(maybeCurrentUser);
        var result = _nominationsMapper.Map(merged);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetNominationsFullDataAsync(CancellationToken cancellationToken)
    {
        var maybeCurrentUser = _domainUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancellationToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancellationToken);
        var merged = maybeNominations.Merge(maybeCurrentUser);
        var maybeNominationsDto = _nominationsMapper.Map(merged);
        var merged2 = maybeNominationsDto.Merge(maybeCurrentUser);
        var result = await _nominationsMapper.EnrichNominationsAsync(merged2, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetPostersAsync(string movieUrl, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieUrl);
        var maybeMovieUrl = validator.Validate(movieUrl);
        var result = await _moviePostersService.GetPosters(maybeMovieUrl, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> DeleteMovieAsync(string movieId, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieId);
        var maybeMovieId = validator.Validate(movieId);
        var maybeCurrentUser = _domainUserAccessor.GetDomainUser(maybeMovieId.AsVoid());
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancellationToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);

        var merged = maybeMovieId.Merge(maybeCurrentUser).Merge(maybeCurrentVotingId).Flatten();
        var result = await _nominationsService.RemoveMovieAsync(merged, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> NominateAsync(NominationDTO dto, CancellationToken cancellationToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<(NominationDTO, DomainUser, CurrentNominationsData)>();
        var maybeCurrentUser = _domainUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancellationToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancellationToken);

        var toValidate = dto.AsMaybe().Merge(maybeCurrentUser).Merge(maybeNominations).Flatten();
        var maybeValidationResult = validator.Validate(toValidate);
        var result = await _nominationsService.NominateAsync(maybeValidationResult, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }
}