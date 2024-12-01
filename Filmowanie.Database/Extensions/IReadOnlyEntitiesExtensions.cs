using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Extensions;

public static class IReadOnlyEntitiesExtensions
{
    public static Vote AsMutable(this IReadOnlyVote vote) => new() { VoteType = vote.VoteType, User = vote.User.AsMutable() };

    public static EmbeddedUser AsMutable(this IReadOnlyEmbeddedUser user) => new() { id = user.id, Name = user.Name, TenantId = user.TenantId };

    public static EmbeddedUserWithNominationAward AsMutable(this IReadOnlyEmbeddedUserWithNominationAward user) =>
        new() { User = user.User.AsMutable(), AwardMessage = user.AwardMessage, Decade = user.Decade };

    public static EmbeddedMovieWithNominationContext AsMutable(this IReadOnlyEmbeddedMovieWithNominationContext movie) => new()
        { Movie = movie.Movie.AsMutable(), NominatedBy = movie.NominatedBy.AsMutable(), NominationConcluded = movie.NominationConcluded, NominationStarted = movie.NominationStarted };

    public static EmbeddedMovieWithVotes AsMutable(this IReadOnlyEmbeddedMovieWithVotes movie) =>
        new() { Movie = movie.Movie.AsMutable(), Votes = movie.Votes.Select(AsMutable), VotingScore = movie.VotingScore };

    public static EmbeddedMovie AsMutable(this IReadOnlyEmbeddedMovie readOnlyEmbeddedMovie) => new()
        { id = readOnlyEmbeddedMovie.id, MovieCreationYear = readOnlyEmbeddedMovie.MovieCreationYear, Name = readOnlyEmbeddedMovie.Name };
};
