using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public interface IEvent
{
    public VotingSessionId VotingSessionId { get; init;}
}