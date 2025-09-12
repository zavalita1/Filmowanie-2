using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Interfaces;

internal interface ICurrentVotingService
{
    Task<Maybe<IReadOnlyMovieEntity[]>> GetCurrentlyVotedMoviesAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken);

    Task<Maybe<IReadOnlyEmbeddedMovieWithVotes[]>> GetCurrentlyVotedMoviesWithVotesAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken);
}