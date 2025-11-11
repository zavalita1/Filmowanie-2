using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Database.Decorators;


internal sealed class FixedTenantUserAccessor : ICurrentUserAccessor
{
    private readonly TenantId tenant;

    public FixedTenantUserAccessor(TenantId tenantId)
    {
        tenant = tenantId;
    }

    public Maybe<DomainUser> GetDomainUser(Maybe<VoidResult> maybe)
    {
        var result = new DomainUser { Tenant = tenant };
        return result.AsMaybe();
    }
}
