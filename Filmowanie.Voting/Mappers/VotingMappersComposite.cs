using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Mappers;

internal sealed class VotingMappersComposite : IVotingMappersComposite
{
    private readonly IVotingSessionsDTOMapper _votingSessionsDtoMapper;
    private readonly IHistoryDtoMapper _historyDtoMapper;
    private readonly IMovieDtoMapper _movieDtoMapper;
    private readonly IVotingSessionIdMapper _votingSessionIdMapper;

    public VotingMappersComposite(IVotingSessionsDTOMapper votingSessionsDtoMapper, IHistoryDtoMapper historyDtoMapper, IMovieDtoMapper movieDtoMapper, IVotingSessionIdMapper votingSessionIdMapper)
    {
        _votingSessionsDtoMapper = votingSessionsDtoMapper;
        _historyDtoMapper = historyDtoMapper;
        _movieDtoMapper = movieDtoMapper;
        _votingSessionIdMapper = votingSessionIdMapper;
    }

    public Task<Maybe<VotingSessionId?>> MapAsync(Maybe<(string, DomainUser)> input, CancellationToken cancelToken) => _votingSessionIdMapper.MapAsync(input, cancelToken);

    public Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input) => _votingSessionsDtoMapper.Map(input);

    public Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input) => _historyDtoMapper.Map(input);

    public Maybe<MovieDTO[]> Map(Maybe<(IReadOnlyMovieEntity[] MoviesEntities, IReadOnlyEmbeddedMovieWithVotes[] EmbeddedMovies, DomainUser CurrentUser)> input) => _movieDtoMapper.Map(input);
}