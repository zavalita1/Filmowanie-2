using Microsoft.Extensions.Logging;

namespace Filmowanie.Abstractions.Interfaces;

public interface IVisitor
{
    public ILogger Log { get; }
}