using Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;
using System.Linq.Expressions;

namespace Filmowanie.Database.Interfaces;

public interface IEventsQueryRepository
{
    public Task<IVotingStartedEvent[]> GetStartedEventsAsync(Expression<Func<IVotingStartedEvent, bool>> predicate, Expression<Func<IVotingConcludedEvent, DateTime>> sortBy, int n,
        CancellationToken cancellationToken);
    public Task<IVotingConcludedEvent[]> GetConcludedEventsAsync(Expression<Func<IVotingConcludedEvent, bool>> predicate, CancellationToken cancellationToken);
    public Task<IVoteRemovedEvent[]> GetVoteAddedEventsAsync(Expression<Func<IVoteRemovedEvent, bool>> predicate, CancellationToken cancellationToken);
    public Task<IVoteRemovedEvent[]> GetVoteRemovedEventsAsync(Expression<Func<IVoteRemovedEvent, bool>> predicate, CancellationToken cancellationToken);
}