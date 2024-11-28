using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;

namespace Filmowanie.Database.Entities.Events;

//internal class VoteAddedEvent : BaseEventEntity, IVoteAddedEvent
//{
//    public VoteType VoteType { get; set; }

//    public string VotingId { get; set; }

//    public EmbeddedUser User { get; set; }
    
//    public EmbeddedMovie Movie { get; set; }

//    IReadOnlyEmbeddedMovie IVoteAddedEvent.Movie => Movie;

//    IReadOnlyEmbeddedUser IVoteAddedEvent.User => User;
//}

//internal class VoteRemovedEvent : BaseEventEntity, IVoteRemovedEvent
//{
//    public VoteType VoteType { get; set; }
//    public string VotingId { get; set; }

//    public EmbeddedUser User { get; set; }

//    IReadOnlyEmbeddedUser IVoteRemovedEvent.User => User;

//    public EmbeddedMovie Movie { get; set; }

//    IReadOnlyEmbeddedMovie IVoteRemovedEvent.Movie => Movie;

//}