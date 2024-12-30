using Filmowanie.Database.Contexts;

namespace Filmowanie.Database.Repositories;

internal sealed class PushSubscriptionRepository
{
    private readonly PushSubscriptionsContext _ctx;

    public PushSubscriptionRepository(PushSubscriptionsContext ctx)
    {
        _ctx = ctx;
    }


}