using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Notification.DTOs.Incoming;
using Filmowanie.Notification.DTOs.Outcoming;
using Filmowanie.Notification.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Filmowanie.Notification.Routes;

internal sealed class PushNotificationRoutes : IPushNotificationRoutes
{
    private readonly ICurrentUserAccessor _userAccessor;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IRoutesResultHelper _routesResultHelper;
    private readonly PushNotificationOptions _options;
    private readonly IFluentValidatorAdapter<PushSubscriptionDTO> _validator;
    
    public PushNotificationRoutes(IFluentValidatorAdapterProvider factory, IOptions<PushNotificationOptions> options, IPushNotificationService pushNotificationService, IRoutesResultHelper routesResultHelper, ICurrentUserAccessor userAccessor)
    {
        _pushNotificationService = pushNotificationService;
        _routesResultHelper = routesResultHelper;
        _userAccessor = userAccessor;
        _options = options.Value;
        _validator = factory.GetAdapter<PushSubscriptionDTO>();
    }

    public async Task<IResult> Add(PushSubscriptionDTO dto, CancellationToken cancelToken)
    {
        var maybeDto = _validator.Validate(dto);
        var currentUser = _userAccessor.GetDomainUser(maybeDto);
        var merged = maybeDto.Merge(currentUser);
        var result = await _pushNotificationService.SavePushNotification(merged, cancelToken);
        var resultDto = result.Map(_ => new AwknowledgedDTO());

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> Notify(NotifyDTO dto, CancellationToken cancelToken)
    {
        var maybeTenant = _userAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var merge = maybeTenant.Merge(dto.Message.AsMaybe()); // TODO dto validation
        var result = await _pushNotificationService.SendAllPushNotificationsAsync(merge, cancelToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public Task<IResult> GetVapidPublicKey(CancellationToken cancelToken) => Task.FromResult<IResult>(TypedResults.Ok(new VapidKeyDTO { Key = _options.PublicKey }));
}