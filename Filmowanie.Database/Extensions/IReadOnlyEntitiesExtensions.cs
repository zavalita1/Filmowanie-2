using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Extensions;

public static class IReadOnlyEntitiesExtensions
{
    internal static Vote AsMutable(this IReadOnlyVote vote) => new(vote);

    internal static EmbeddedUser AsMutable(this IReadOnlyEmbeddedUser user) => new(user);

    internal static EmbeddedUserWithNominationAward AsMutable(this IReadOnlyEmbeddedUserWithNominationAward user) => new(user);

    internal static EmbeddedMovieWithNominationContext AsMutable(this IReadOnlyEmbeddedMovieWithNominationContext movie) => new(movie);
    internal static EmbeddedMovieWithNominatedBy AsMutable(this IReadOnlyEmbeddedMovieWithNominatedBy movie) => new(movie);

    internal static EmbeddedMovieWithVotes AsMutable(this IReadOnlyEmbeddedMovieWithVotes movie) => new(movie);
    internal static EmbeddedMovie AsMutable(this IReadOnlyEmbeddedMovie readOnlyEmbeddedMovie) => new(readOnlyEmbeddedMovie);

    internal static MovieEntity AsMutable(this IReadOnlyMovieEntity entity) => new(entity);
    internal static NominatedMovieEvent AsMutable(this IReadOnlyNominatedMovieEvent entity) => new(entity);
    internal static UserEntity AsMutable(this IReadOnlyUserEntity entity) => new(entity);
    internal static VotingResult AsMutable(this IReadOnlyVotingResult entity) => new(entity);
};
