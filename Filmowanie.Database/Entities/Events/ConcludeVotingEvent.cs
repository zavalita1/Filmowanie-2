using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;
using MassTransit;
using Newtonsoft.Json;

namespace Filmowanie.Database.Entities.Events;

//internal class VotingConcludedEvent : BaseEventEntity, IVotingConcludedEvent
//{
//    public MovieEntity[] Movies { get; set; }
//    public string VotingId { get; set; }

//    IReadOnlyMovieEntity[] IVotingConcludedEvent.Movies => Movies;
//}

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
}

public sealed class NominationData
{
    public DomainUser User { get; init; }
    public int Year { get; init; }
    public DateTime? Concluded { get; set; }
}

public record StartVotingEvent(Guid CorrelationId, EmbeddedMovie[] Movies, NominationData[] NominationsData) : IEvent { }
public record AddMovieEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record RemoveMovieEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record RemoveVoteEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record VoteRemovedEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record AddVoteEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User, VoteType VoteType) : IEvent { }
public record VoteAddedEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record AddNominationsEvent(Guid CorrelationId, NominationData[] NominationsData) : IEvent { }

public record ConcludeVotingEvent(Guid CorrelationId) : IEvent { }
public record VotingConcludedEvent(Guid CorrelationId) : IEvent { }
public record VotingStartingEvent(Guid CorrelationId) : IEvent { }
public record NominationAddedEvent(Guid CorrelationId, NominationData NominationData) : IEvent { }

public record MoviesListRequested(Guid CorrelationId) : IEvent { }
public record NominationsRequested(Guid CorrelationId) : IEvent { }

public interface IEvent
{
    public Guid CorrelationId { get; init;}
}