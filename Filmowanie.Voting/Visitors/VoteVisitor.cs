using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class VoteVisitor : IVoteVisitor
{
    private readonly IBus _bus;
    private readonly ILogger<VoteVisitor> _log;

    public VoteVisitor(IBus bus, ILogger<VoteVisitor> log)
    {
        _bus = bus;
        _log = log;
    }

    public async Task<OperationResult<object>> VisitAsync(OperationResult<(DomainUser, VotingSessionId, VoteDTO)> input, CancellationToken cancellationToken)
    {
        var (user, votingSessionId, voteDto) = input.Result;
        var movie = new EmbeddedMovie { id = voteDto.MovieId, Name = voteDto.MovieTitle };

        if (voteDto.Votes == 0)
        {
            var removeVoteEvent = new RemoveVoteEvent(votingSessionId.CorrelationId, movie, user);
            await _bus.Publish(removeVoteEvent, cancellationToken);
        }
        else
        {
            var @event = new AddVoteEvent(votingSessionId.CorrelationId, movie, user, (VoteType)voteDto.Votes);
            await _bus.Publish(@event, cancellationToken);
        }
        
        return new OperationResult<object>(default, null);
    }

    public ILogger Log => _log;
}