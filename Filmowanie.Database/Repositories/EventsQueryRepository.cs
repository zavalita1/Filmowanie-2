using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Filmowanie.Database.Repositories;

internal class EventsQueryRepository : IEventsQueryRepository
{
    private readonly EventsContext _ctx;

    public EventsQueryRepository(EventsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IVotingStartedEvent[]> GetStartedEventsAsync(Expression<Func<IVotingStartedEvent, bool>> predicate, Expression<Func<IVotingConcludedEvent, DateTime>> sortBy, int n,
        CancellationToken cancellationToken)
    {
        return await _ctx.VotingStatedEvents.Where(predicate).OrderByDescending(predicate).Take(n).ToArrayAsync(cancellationToken);
    }

    public async Task<IVotingConcludedEvent[]> GetConcludedEventsAsync(Expression<Func<IVotingConcludedEvent, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _ctx.VotingConcludedEvents.Where(predicate).ToArrayAsync(cancellationToken);
    }

    public async Task<IVoteRemovedEvent[]> GetVoteAddedEventsAsync(Expression<Func<IVoteRemovedEvent, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _ctx.VoteRemovedEvents.Where(predicate).ToArrayAsync(cancellationToken);
    }

    public async Task<IVoteRemovedEvent[]> GetVoteRemovedEventsAsync(Expression<Func<IVoteRemovedEvent, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _ctx.VoteRemovedEvents.Where(predicate).ToArrayAsync(cancellationToken);
    }
}