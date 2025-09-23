using Filmowanie.Abstractions.Configuration;
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
    private readonly ICurrentUserAccessor userAccessor;
    private readonly IPushNotificationService pushNotificationService;
    private readonly IRoutesResultHelper routesResultHelper;
    private readonly PushNotificationOptions options;
    private readonly IFluentValidatorAdapter<PushSubscriptionDTO> validator;
    
    public PushNotificationRoutes(IFluentValidatorAdapterProvider factory, IOptions<PushNotificationOptions> options, IPushNotificationService pushNotificationService, IRoutesResultHelper routesResultHelper, ICurrentUserAccessor userAccessor)
    {
        this.pushNotificationService = pushNotificationService;
        this.routesResultHelper = routesResultHelper;
        this.userAccessor = userAccessor;
        this.options = options.Value;
        validator = factory.GetAdapter<PushSubscriptionDTO>();
    }

    public async Task<IResult> Add(PushSubscriptionDTO dto, CancellationToken cancelToken)
    {
        var maybeDto = this.validator.Validate(dto);
        var currentUser = this.userAccessor.GetDomainUser(maybeDto);
        var result = await this.pushNotificationService.SavePushNotification(maybeDto, currentUser, cancelToken);
        var resultDto = result.Map(_ => new AwknowledgedDTO());

        return this.routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> Notify(NotifyDTO dto, CancellationToken cancelToken)
    {
        var maybeTenant = this.userAccessor.GetDomainUser(VoidResult.Void).Map(x => x.Tenant);
        var result = await this.pushNotificationService.SendAllPushNotificationsAsync(maybeTenant, dto.Message.AsMaybe(), cancelToken);

        return this.routesResultHelper.UnwrapOperationResult(result);
    }

    public Task<IResult> GetVapidPublicKey(CancellationToken cancelToken) => Task.FromResult<IResult>(TypedResults.Ok(new VapidKeyDTO { Key = this.options.PublicKey }));
}