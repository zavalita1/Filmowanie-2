using Filmowanie.Abstractions.Enums;
using MassTransit;
using Newtonsoft.Json;

namespace Filmowanie.Database.Entities.Voting;

public class VotingStateInstance : SagaStateMachineInstance
{
    [JsonProperty("id")]
    public Guid CorrelationId { get; set; }

    public string? CurrentState { get; set; }

    public IEnumerable<EmbeddedMovieWithVotes> Movies { get; set; } = [];

    public IEnumerable<NominationData> Nominations { get; set; } = [];

    [JsonProperty("_etag")]
    public string? ETag { get; set; }

    public int TenantId { get; set; }

    public DateTime Created { get; set; }

    public ErrorData? Error { get; set; }
}

public sealed class NominationData
{
    public NominationDataEmbeddedUser? User { get; init; }
    public Decade Year { get; init; }
    public DateTime? Concluded { get; set; }

    public string? MovieId { get; set; }
}

public sealed class NominationDataEmbeddedUser
{
    public string? DisplayName { get; set; }

    public string? Id { get; set; }
}

public class ErrorData
{
    public string? ErrorMessage { get; set; }
    public string? CallStack { get; set; }
}