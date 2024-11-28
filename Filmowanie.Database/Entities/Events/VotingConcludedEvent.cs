using Filmowanie.Abstractions;
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

    public IEnumerable<TempNominationData> Nominations { get; set; }

    [JsonProperty("_etag")]
    public string ETag { get; set; }

    public int TenantId { get; set; }
}

public record TempNominationData(DomainUser User, int Year);

public record StartVotingEvent(Guid CorrelationId, EmbeddedMovie[] Movies, TempNominationData[] NominationsData) : IEvent { }
public record AddMovieEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record RemoveMovieEvent(Guid CorrelationId, EmbeddedMovie Movie, DomainUser User) : IEvent { }
public record AddNominationsEvent(Guid CorrelationId, TempNominationData[] NominationsData) : IEvent { }

public record VotingConcludedEvent(Guid CorrelationId) : IEvent { }
public record VotingStartingEvent(Guid CorrelationId) : IEvent { }
public record NominationAddedEvent(Guid CorrelationId, TempNominationData NominationData) : IEvent { }

public record MoviesListRequested(Guid CorrelationId) : IEvent { }
public record NominationsRequested(Guid CorrelationId) : IEvent { }

public interface IEvent
{
    public Guid CorrelationId { get; init;}
}