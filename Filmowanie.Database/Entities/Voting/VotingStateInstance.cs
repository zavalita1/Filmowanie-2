using MassTransit;
using Newtonsoft.Json;

namespace Filmowanie.Database.Entities.Voting;

public class VotingStateInstance : SagaStateMachineInstance
{
    [JsonProperty("id")]
    public Guid CorrelationId { get; set; }

    public string? CurrentState { get; set; }

    public IEnumerable<EmbeddedMovieWithVotes> Movies { get; set; } = [];
    public IEnumerable<EmbeddedMovieWithVotes>? ExtraVotingMovies { get; set; } = null;

    public IEnumerable<NominationData> Nominations { get; set; } = [];

    [JsonProperty("_etag")]
    public string? ETag { get; set; }

    public int TenantId { get; set; }

    public DateTime Created { get; set; }

    public ErrorData? Error { get; set; }
}