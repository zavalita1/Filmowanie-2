using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

internal sealed class VoteService : IVoteService
{
    private readonly IBus _bus;
    private readonly ILogger<VoteService> _log;

    public VoteService(IBus bus, ILogger<VoteService> log)
    {
        _bus = bus;
        _log = log;
    }

    public Task<Maybe<VoidResult>> VoteAsync(Maybe<(DomainUser, VotingSessionId, VoteDTO)> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(VoteAsync, _log, cancellationToken);

    public async Task<Maybe<VoidResult>> VoteAsync((DomainUser, VotingSessionId, VoteDTO) input, CancellationToken cancellationToken)
    {
        var (user, votingSessionId, voteDto) = input;
        var movie = new EmbeddedMovie { id = voteDto.MovieId, Name = voteDto.MovieTitle };

        if (voteDto.Votes == 0)
        {
            var removeVoteEvent = new RemoveVoteEvent(votingSessionId, movie, user);
            await _bus.Publish(removeVoteEvent, cancellationToken);
        }
        else
        {
            var @event = new AddVoteEvent(votingSessionId, movie, user, (VoteType)voteDto.Votes);
            await _bus.Publish(@event, cancellationToken);
        }

        return VoidResult.Void;
    }
}