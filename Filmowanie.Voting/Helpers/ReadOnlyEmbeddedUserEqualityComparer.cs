using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Helpers;

// TODO UTs
public class ReadOnlyEmbeddedUserEqualityComparer : IEqualityComparer<IReadOnlyEmbeddedUser>
{
    public static ReadOnlyEmbeddedUserEqualityComparer Instance => new ();

    public bool Equals(IReadOnlyEmbeddedUser? x, IReadOnlyEmbeddedUser? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.id == y.id && x.Name == y.Name && x.TenantId == y.TenantId;
    }

    public int GetHashCode(IReadOnlyEmbeddedUser obj)
    {
        return HashCode.Combine(obj.id, obj.Name, obj.TenantId);
    }
}