using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Database.Entities.Voting;

public sealed class NominationData
{
    public NominationDataEmbeddedUser? User { get; init; }
    public Decade Year { get; init; }
    public DateTime? Concluded { get; set; }

    public string? MovieId { get; set; }
}