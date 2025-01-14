using Filmowanie.Abstractions.Interfaces;

namespace Filmowanie.Abstractions.Wrappers;

public class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}