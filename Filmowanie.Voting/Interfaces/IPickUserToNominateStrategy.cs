using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Interfaces;

public interface IPickUserToNominateStrategy
{
    IReadOnlyEmbeddedUser GetUserToNominate(IReadOnlyEmbeddedMovie movieToReplace, IDictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> userContexts);
}

public sealed class PickUserToNominateContext
{
    public double AverageNominationPendingTimeInDays { get; set; }

    public int NominationsCount { get; set; }

    public double ParticipationPercent { get; set; }

    public (string MovieId, VoteType)[] Votes { get; set; }
}