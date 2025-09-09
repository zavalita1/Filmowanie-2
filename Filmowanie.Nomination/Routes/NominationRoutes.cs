using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Routes;

internal sealed class NominationRoutes : INominationRoutes
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ICurrentVotingSessionIdAccessor _currentVotingSessionIdAccessor;
    private readonly INominationsService _nominationsService;
    private readonly INominationsMapper _nominationsMapper;
    private readonly IMoviePostersService _moviePostersService;

    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IRoutesResultHelper _routesResultHelper;

    public NominationRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IMoviePostersService moviePostersService, IRoutesResultHelper routesResultHelper, ICurrentUserAccessor currentUserAccessor, ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor, INominationsService nominationsService, INominationsMapper nominationsMapper)
    {
        _validatorAdapterProvider = validatorAdapterProvider;
        _moviePostersService = moviePostersService;
        _routesResultHelper = routesResultHelper;
        _currentUserAccessor = currentUserAccessor;
        _currentVotingSessionIdAccessor = currentVotingSessionIdAccessor;
        _nominationsService = nominationsService;
        _nominationsMapper = nominationsMapper;
    }

    public async Task<IResult> GetNominationsAsync(CancellationToken cancelToken)
    {
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);
        var merged = maybeNominations.Merge(maybeCurrentUser);
        var result = _nominationsMapper.Map(merged);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetNominationsFullDataAsync(CancellationToken cancelToken)
    {
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);
        var merged = maybeNominations.Merge(maybeCurrentUser);
        var maybeNominationsDto = _nominationsMapper.Map(merged);
        var result = await _nominationsMapper.EnrichNominationsAsync(maybeNominationsDto, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetPostersAsync(string movieUrl, CancellationToken cancelToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieUrl);
        var maybeMovieUrl = validator.Validate(movieUrl);
        var result = await _moviePostersService.GetPosters(maybeMovieUrl, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> DeleteMovieAsync(string movieId, CancellationToken cancelToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieId);
        var maybeMovieId = validator.Validate(movieId);
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(maybeMovieId);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);

        var merged = maybeMovieId.Merge(maybeCurrentUser).Merge(maybeCurrentVotingId).Flatten();
        var result = await _nominationsService.RemoveMovieAsync(merged, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> NominateAsync(NominationDTO dto, CancellationToken cancelToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<(NominationDTO, DomainUser, CurrentNominationsData)>();
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);

        var toValidate = dto.AsMaybe().Merge(maybeCurrentUser).Merge(maybeNominations).Flatten();
        var maybeValidationResult = validator.Validate(toValidate);
        var result = await _nominationsService.NominateAsync(maybeValidationResult, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }
}