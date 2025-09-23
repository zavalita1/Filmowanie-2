using Filmowanie.Abstractions.DomainModels;
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
    private readonly ICurrentUserAccessor currentUserAccessor;
    private readonly ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor;
    private readonly INominationsService nominationsService;
    private readonly INominationsEnricher nominationsEnricher;
    private readonly INominationsMapper nominationsMapper;
    private readonly IMoviePostersService moviePostersService;
    private readonly IFilmwebHandler filmwebHandler;

    private readonly IFluentValidatorAdapterProvider validatorAdapterProvider;
    private readonly IRoutesResultHelper routesResultHelper;

    public NominationRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IMoviePostersService moviePostersService, IRoutesResultHelper routesResultHelper, ICurrentUserAccessor currentUserAccessor, ICurrentVotingSessionIdAccessor currentVotingSessionIdAccessor, INominationsService nominationsService, INominationsEnricher nominationsEnricher, IFilmwebHandler filmwebHandler, INominationsMapper nominationsMapper)
    {
        this.validatorAdapterProvider = validatorAdapterProvider;
        this.moviePostersService = moviePostersService;
        this.routesResultHelper = routesResultHelper;
        this.currentUserAccessor = currentUserAccessor;
        this.currentVotingSessionIdAccessor = currentVotingSessionIdAccessor;
        this.nominationsService = nominationsService;
        this.nominationsEnricher = nominationsEnricher;
        this.filmwebHandler = filmwebHandler;
        this.nominationsMapper = nominationsMapper;
    }

    public async Task<IResult> GetNominationsAsync(CancellationToken cancelToken)
    {
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await this.currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);

        if (maybeNullableCurrentVotingId.Result == null)
            return this.routesResultHelper.UnwrapOperationResult(new NominationsDataDTO { Nominations = [] }.AsMaybe());

        var maybeCurrentVotingId = this.currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await this.nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);
        var result = this.nominationsMapper.Map(maybeNominations, maybeCurrentUser);

        return this.routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetNominationsFullDataAsync(CancellationToken cancelToken)
    {
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await this.currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = this.currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await this.nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);
        var maybeNominationsDto = this.nominationsMapper.Map(maybeNominations, maybeCurrentUser);
        var result = await this.nominationsEnricher.EnrichNominationsAsync(maybeNominationsDto, cancelToken);

        return this.routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetPostersAsync(string movieUrl, CancellationToken cancelToken)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieUrl);
        var maybeMovieUrl = validator.Validate(movieUrl);
        var result = await this.moviePostersService.GetPosters(maybeMovieUrl, cancelToken);

        return this.routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> ResetNominationAsync(string movieId, CancellationToken cancelToken)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<string>(KeyedServices.MovieId);
        var maybeMovieId = validator.Validate(movieId);
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(maybeMovieId);
        var maybeNullableCurrentVotingId = await this.currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = this.currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);

        var result = await this.nominationsService.ResetNominationAsync(maybeMovieId, maybeCurrentUser, maybeCurrentVotingId, cancelToken);

        return this.routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> NominateAsync(NominationDTO dto, CancellationToken cancelToken)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<(NominationDTO, DomainUser, CurrentNominationsData)>();
        var movieValidator = this.validatorAdapterProvider.GetAdapter<(IReadOnlyMovieEntity, DomainUser, CurrentNominationsData)>();
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableCurrentVotingId = await this.currentVotingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);
        var maybeCurrentVotingId = this.currentVotingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableCurrentVotingId);
        var maybeNominations = await this.nominationsService.GetNominationsAsync(maybeCurrentVotingId, cancelToken);

        var maybeDto = dto.AsMaybe();
        var toValidate = maybeDto.Merge(maybeCurrentUser, maybeNominations);
        var maybeValidationResult = validator.Validate(toValidate).Map(x => (x.Item1, x.Item2));
        var maybeMovie = await this.filmwebHandler.GetMovieAsync(maybeValidationResult, cancelToken);
        var movieToValidate = maybeMovie.Merge(maybeCurrentUser, maybeNominations);
        var maybeMoviePostValidation = movieValidator.Validate(movieToValidate).Map(x => x.Item1);

        var result = await this.nominationsService.NominateAsync(maybeMoviePostValidation, maybeCurrentUser, maybeCurrentVotingId, cancelToken);

        return this.routesResultHelper.UnwrapOperationResult(result);
    }
}