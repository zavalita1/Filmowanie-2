using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Interfaces;

public interface IPickUserToNominateStrategy
{
    DomainUser GetUserToNominate(IReadOnlyEmbeddedMovie movieToReplace, IDictionary<string, PickUserToNominateContext> userContexts);
}

public sealed class PickUserToNominateContext
{
    public double AverageNominationPendingTime { get; set; }

    public int NominationsCount { get; set; }

    public double ParticipationPercent { get; set; }

    public (string MovieId, VoteType)[] Votes { get; set; }
}