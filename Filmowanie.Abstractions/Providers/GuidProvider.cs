using Filmowanie.Abstractions.Interfaces;

namespace Filmowanie.Abstractions.Providers;

public class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}