using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Models;

internal sealed class VotingResult : IReadonlyVotingResult
{
    public string Id { get; init; }
    public DateTime Created { get; init; }
    public int TenantId { get; init; }
    public DateTime? Concluded { get; init; }
    public IResultEmbeddedMovie[] Movies { get; init; }
    public IReadOnlyEmbeddedUserWithNominationAward[] UsersAwardedWithNominations { get; init; }
    public IReadOnlyEmbeddedMovie[] MoviesGoingByeBye { get; init; }
    public IReadOnlyEmbeddedMovieWithNominationContext[] MoviesAdded { get; init; }
    public IReadOnlyEmbeddedMovie Winner { get; }
}
