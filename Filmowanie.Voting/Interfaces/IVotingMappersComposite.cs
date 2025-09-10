using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingMappersComposite
{
    Task<Maybe<VotingSessionId?>> MapAsync(Maybe<(string, DomainUser)> input, CancellationToken cancelToken);

    Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input);
    
    Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input);

    Maybe<MovieDTO[]> Map(Maybe<(IReadOnlyMovieEntity[] MoviesEntities, IReadOnlyEmbeddedMovieWithVotes[] EmbeddedMovies, DomainUser CurrentUser)> input);
}