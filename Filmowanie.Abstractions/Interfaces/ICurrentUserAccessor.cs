using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Abstractions.Interfaces;

/// <summary>
/// Component for retrieving currently logged user domain data model.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Retrieving currently logged user domain data model.
    /// </summary>
    /// <param name="maybe">Discard value</param>
    /// <returns>Domain data model.</returns>
    Maybe<DomainUser> GetDomainUser(Maybe<VoidResult> maybe);
}