using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingResultsRetriever
{
    VotingResults GetVotingResults(IReadOnlyEmbeddedMovieWithVotes[] currentMovies, IReadOnlyVotingResult? previous);
}