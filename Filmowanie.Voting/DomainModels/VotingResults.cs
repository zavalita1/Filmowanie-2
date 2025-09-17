using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.DomainModels;

public readonly record struct VotingResults(IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IReadOnlyEmbeddedMovieWithVotes[] Movies, IReadOnlyEmbeddedMovie Winner);