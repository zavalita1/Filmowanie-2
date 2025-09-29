using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;

namespace Filmowanie.Voting.Interfaces;

public interface IVotingResultInterpreter
{
    bool IsExtraVotingNecessary(VotingResults results, out IReadOnlyEmbeddedMovie[] qualifiedMovies);
}