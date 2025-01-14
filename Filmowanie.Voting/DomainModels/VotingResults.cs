using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.DomainModels;

internal readonly record struct VotingResults(IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IReadOnlyEmbeddedMovieWithVotes[] Movies, IReadOnlyEmbeddedMovie Winner);