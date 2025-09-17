using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Abstractions.Interfaces;

/// <summary>
/// Provides access to the current voting session identifier.
/// </summary>
public interface ICurrentVotingSessionIdAccessor
{
    /// <summary>
    /// Asynchronously retrieves the current voting session identifier for a given user. If current voting is stopped (e.g. before after conclusion), it will return null.
    /// </summary>
    /// <param name="maybeCurrentUser">The current user's domain model wrapped in a Maybe type.</param>
    /// <param name="cancelToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the voting session ID wrapped in a Maybe type.</returns>
    Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken);

    /// <summary>
    /// Gets the required voting session identifier from a nullable maybe value.
    /// </summary>
    /// <param name="maybeCurrentVotingSessionId">The nullable voting session ID wrapped in a Maybe type.</param>
    /// <returns>A non-nullable voting session ID wrapped in a Maybe type.</returns>
    Maybe<VotingSessionId> GetRequiredVotingSessionId(Maybe<VotingSessionId?> maybeCurrentVotingSessionId);

    Task<Maybe<VotingSessionId>> GetLastVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken);
}