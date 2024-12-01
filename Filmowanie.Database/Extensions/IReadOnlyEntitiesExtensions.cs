using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Extensions;

internal static class IReadOnlyEntitiesExtensions
{
    public static ResultEmbeddedMovie AsMutable(this IResultEmbeddedMovie movie) => new() { Movie = movie.Movie, VotingScore = movie.VotingScore };

    public static Vote AsMutable(this IReadOnlyVote vote) => new() { VoteType = vote.VoteType, User = vote.User.AsMutable() };

    public static EmbeddedUser AsMutable(this IReadOnlyEmbeddedUser user) => new() { id = user.id, Name = user.Name, TenantId = user.TenantId };
    public static EmbeddedUserWithNominationAward AsMutable(this IReadOnlyEmbeddedUserWithNominationAward user) => new() { id = user.id, Name = user.Name, TenantId = user.TenantId, AwardMessage = user.AwardMessage, Decade = user.Decade };
    public static EmbeddedMovieWithNominationContext AsMutable(this IReadOnlyEmbeddedMovieWithNominationContext user) => new() { id = user.id, Name = user.Name, NominatedBy = user.NominatedBy.AsMutable(), NominationConcluded = user.NominationConcluded, NominationStarted = user.NominationStarted};
}