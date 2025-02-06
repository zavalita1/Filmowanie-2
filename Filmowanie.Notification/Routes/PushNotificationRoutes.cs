using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Notification.DTOs.Incoming;
using Filmowanie.Notification.DTOs.Outcoming;
using Filmowanie.Notification.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Filmowanie.Notification.Routes;

internal sealed class PushNotificationRoutes : IPushNotificationRoutes
{
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly ISavePushNotificationVisitor _savePushNotificationVisitor;
    private readonly INotifyAllPushSubscribersVisitor _notifyAllPushSubscribersVisitor;
    private readonly IRoutesResultHelper _routesResultHelper;
    private readonly PushNotificationOptions _options;
    private readonly IFluentValidatorAdapter<PushSubscriptionDTO> _validator;
    
    public PushNotificationRoutes(IFluentValidatorAdapterProvider factory, IUserIdentityVisitor userIdentityVisitor, IOptions<PushNotificationOptions> options, ISavePushNotificationVisitor savePushNotificationVisitor, INotifyAllPushSubscribersVisitor notifyAllPushSubscribersVisitor, IRoutesResultHelper routesResultHelper)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _savePushNotificationVisitor = savePushNotificationVisitor;
        _notifyAllPushSubscribersVisitor = notifyAllPushSubscribersVisitor;
        _routesResultHelper = routesResultHelper;
        _options = options.Value;
        _validator = factory.GetAdapter<PushSubscriptionDTO>();
    }

    public async Task<IResult> Add(PushSubscriptionDTO dto, CancellationToken cancellationToken)
    {
        var userResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);
        var result = (await dto.ToOperationResult()
            .Accept(_validator)
            .Merge(userResult)
            .AcceptAsync(_savePushNotificationVisitor, cancellationToken))
            .Accept(_ => new AwknowledgedDTO());

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> Notify(NotifyDTO dto, CancellationToken cancellationToken)
    {
        var tenantResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor).Pluck(x => x.Tenant);
        var dtoResult = dto.ToOperationResult().Pluck(x => x.Message);
        var result = await tenantResult.Merge(dtoResult).AcceptAsync(_notifyAllPushSubscribersVisitor, cancellationToken);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public Task<IResult> GetVapidPublicKey(CancellationToken cancellationToken) => Task.FromResult<IResult>(TypedResults.Ok(new VapidKeyDTO { Key = _options.PublicKey }));


}