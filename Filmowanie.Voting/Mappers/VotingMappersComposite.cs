using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Mappers;

// TODO UTs
internal sealed class VotingMappersComposite : IVotingMappersComposite
{
    private readonly IVotingSessionsDTOMapper votingSessionsDtoMapper;
    private readonly IHistoryDtoMapper historyDtoMapper;
    private readonly IMovieDtoMapper movieDtoMapper;
    private readonly IVotingSessionIdMapper votingSessionIdMapper;

    public VotingMappersComposite(IVotingSessionsDTOMapper votingSessionsDtoMapper, IHistoryDtoMapper historyDtoMapper, IMovieDtoMapper movieDtoMapper, IVotingSessionIdMapper votingSessionIdMapper)
    {
        this.votingSessionsDtoMapper = votingSessionsDtoMapper;
        this.historyDtoMapper = historyDtoMapper;
        this.movieDtoMapper = movieDtoMapper;
        this.votingSessionIdMapper = votingSessionIdMapper;
    }

    public Task<Maybe<VotingSessionId>> MapAsync(Maybe<string> maybeVotingId, Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken) => this.votingSessionIdMapper.MapAsync(maybeVotingId.Merge(maybeCurrentUser), cancelToken);

    public Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input) => this.votingSessionsDtoMapper.Map(input);

    public Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input) => this.historyDtoMapper.Map(input);

    public Maybe<MovieDTO[]> Map(Maybe<IReadOnlyMovieEntity[]> maybeMovies, Maybe<IReadOnlyEmbeddedMovieWithVotes[]> maybeMovieWithVotes, Maybe<DomainUser> maybeCurrentUser) => this.movieDtoMapper.Map(maybeMovies.Merge(maybeMovieWithVotes).Merge(maybeCurrentUser).Flatten());
}