using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting;

public interface IEvent
{
    public VotingSessionId VotingSessionId { get; init;}
}