namespace Filmowanie.Database.Entities.Voting;

public class CurrentNominationsResponse
{
    public Guid CorrelationId { get; set; }

    public NominationData[] Nominations { get; set; } = null!;
}
