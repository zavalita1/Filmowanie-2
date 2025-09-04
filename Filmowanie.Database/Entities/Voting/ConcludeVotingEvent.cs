using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using MassTransit;
using Newtonsoft.Json;

namespace Filmowanie.Database.Entities.Voting;

public class VotingStateInstance : SagaStateMachineInstance
{
    [JsonProperty("id")]
    public Guid CorrelationId { get; set; }

    public string CurrentState { get; set; }

    public IEnumerable<EmbeddedMovieWithVotes> Movies { get; set; }

    public IEnumerable<NominationData> Nominations { get; set; }

    [JsonProperty("_etag")]
    public string ETag { get; set; }

    public int TenantId { get; set; }

    public DateTime Created { get; set; }

    public ErrorData? Error { get; set; }
}

public sealed class NominationData
{
    public NominationDataEmbeddedUser User { get; init; }
    public Decade Year { get; init; }
    public DateTime? Concluded { get; set; }

    public string? MovieId { get; set; }
}

public sealed class NominationDataEmbeddedUser
{
    public string DisplayName { get; set; }

    public string Id { get; set; }
}

public class ErrorData
{
    public string ErrorMessage { get; set; }
    public string? CallStack { get; set; }
}

public record StartVotingEvent(Guid CorrelationId, EmbeddedMovie[] Movies, NominationData[] NominationsData, DateTime Created, TenantId TenantId) : IEvent { }

public record RemoveVoteEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record VoteRemovedEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record AddVoteEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User, VoteType VoteType) : IEvent { }
public record VoteAddedEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record AddNominationsEvent(Guid CorrelationId, NominationData[] NominationsData) : IEvent { }

public record ConcludeVotingEvent(Guid CorrelationId, TenantId Tenant) : IEvent { }
public record VotingConcludedEvent(Guid CorrelationId, TenantId Tenant, IReadOnlyEmbeddedMovieWithVotes[] MoviesWithVotes, NominationData[] NominationsData, DateTime VotingStarted) : IEvent { }
public record VotingStartingEvent(Guid CorrelationId) : IEvent { }
public record NominationAddedEvent(Guid CorrelationId, NominationData NominationData) : IEvent { }

public record MoviesListRequested(Guid CorrelationId) : IEvent { }

public record ResultsCalculated(Guid CorrelationId) : IEvent { }
public record ErrorEvent(Guid CorrelationId, string Message, string? CallStack) : IEvent { }