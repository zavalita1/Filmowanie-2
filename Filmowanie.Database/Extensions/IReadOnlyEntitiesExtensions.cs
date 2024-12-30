using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Extensions;

public static class IReadOnlyEntitiesExtensions
{
    public static Vote AsMutable(this IReadOnlyVote vote) => new(vote);

    public static EmbeddedUser AsMutable(this IReadOnlyEmbeddedUser user) => new(user);

    public static EmbeddedUserWithNominationAward AsMutable(this IReadOnlyEmbeddedUserWithNominationAward user) => new(user);

    public static EmbeddedMovieWithNominationContext AsMutable(this IReadOnlyEmbeddedMovieWithNominationContext movie) => new(movie);

    public static EmbeddedMovieWithVotes AsMutable(this IReadOnlyEmbeddedMovieWithVotes movie) => new(movie);
    public static EmbeddedMovie AsMutable(this IReadOnlyEmbeddedMovie readOnlyEmbeddedMovie) => new(readOnlyEmbeddedMovie);

    internal static MovieEntity AsMutable(this IReadOnlyMovieEntity entity) => new(entity);
    internal static NominatedMovieAgainEvent AsMutable(this IReadOnlyNominatedMovieAgainEvent entity) => new(entity);
    internal static UserEntity AsMutable(this IReadOnlyUserEntity entity) => new(entity);
    internal static VotingResult AsMutable(this IReadOnlyVotingResult entity) => new(entity);
};
