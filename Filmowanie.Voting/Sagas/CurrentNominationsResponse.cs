using Filmowanie.Database.Entities.Voting;

namespace Filmowanie.Voting.Sagas;

public class CurrentNominationsResponse
{
    public NominationData[] Nominations { get; set; }
}