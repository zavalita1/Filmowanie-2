using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Models;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Routes;

internal sealed class NominationRoutes : INominationRoutes
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ICurrentVotingSessionIdAccessor _currentVotingSessionIdAccessor;
    private readonly INominationsService _nominationsService;
    private readonly INominationsEnricher _nominationsEnricher;
    private readonly INominationsMapper _nominationsMapper;
    private readonly IMoviePostersService _moviePostersService;
    private readonly IFilmwebHandler _filmwebHandler;

    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IRoutesResultHelper _routesResultHelper;

    public NominationRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IMoviePostersService moviePostersService, IRoutesResultHelper routesResultHelper, ICurrentUserAccessor currentUserAccessor, ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor, INominationsService nominationsService, INominationsEnricher nominationsEnricher, IFilmwebHandler filmwebHandler, INominationsMapper nominationsMapper)
    {
        _validatorAdapterProvider = validatorAdapterProvider;
        _moviePostersService = moviePostersService;
        _routesResultHelper = routesResultHelper;
        _currentUserAccessor = currentUserAccessor;
        _currentVotingSessionIdAccessor = currentVotingSessionIdAccessor;
        _nominationsService = nominationsService;
        _nominationsEnricher = nominationsEnricher;
        _filmwebHandler = filmwebHandler;
        _nominationsMapper = nominationsMapper;
    }

    public async Task<IResult> GetNominationsAsync(CancellationToken cancelToken)
    {
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);

        if (maybeNullableCurrentVotingId.Result == null)
            return _routesResultHelper.UnwrapOperationResult(new NominationsDataDTO { Nominations = [] }.AsMaybe());

        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);
        var result = _nominationsMapper.Map(maybeNominations, maybeCurrentUser);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetNominationsFullDataAsync(CancellationToken cancelToken)
    {
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);
        var maybeNominationsDto = _nominationsMapper.Map(maybeNominations, maybeCurrentUser);
        var result = await _nominationsEnricher.EnrichNominationsAsync(maybeNominationsDto, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetPostersAsync(string movieUrl, CancellationToken cancelToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieUrl);
        var maybeMovieUrl = validator.Validate(movieUrl);
        var result = await _moviePostersService.GetPosters(maybeMovieUrl, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> ResetNominationAsync(string movieId, CancellationToken cancelToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieId);
        var maybeMovieId = validator.Validate(movieId);
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(maybeMovieId);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);

        var result = await _nominationsService.ResetNominationAsync(maybeMovieId, maybeCurrentUser, maybeCurrentVotingId, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> NominateAsync(NominationDTO dto, CancellationToken cancelToken)
    {
        var validator = _validatorAdapterProvider.GetAdapter<(NominationDTO, DomainUser, CurrentNominationsData)>();
        var movieValidator = _validatorAdapterProvider.GetAdapter<(IReadOnlyMovieEntity, DomainUser, CurrentNominationsData)>();
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await _currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = _currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await _nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);

        var maybeDto = dto.AsMaybe();
        var toValidate = maybeDto.Merge(maybeCurrentUser, maybeNominations);
        var maybeValidationResult = validator.Validate(toValidate).Map(x => (x.Item1, x.Item2));
        var maybeMovie = await _filmwebHandler.GetMovieAsync(maybeValidationResult, cancelToken);
        var movieToValidate = maybeMovie.Merge(maybeCurrentUser, maybeNominations);
        var maybeMoviePostValidation = movieValidator.Validate(movieToValidate).Map(x => x.Item1);

        var result = await _nominationsService.NominateAsync(maybeMoviePostValidation, maybeCurrentUser, maybeCurrentVotingId, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }
}