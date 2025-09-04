using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public interface IEvent
{
    public VotingSessionId VotingSessionId { get; init;}
}