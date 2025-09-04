using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Services;

internal sealed class MoviePostersService : IMoviePostersService
{
    private readonly ILogger<MoviePostersService> _log;
    private readonly IFilmwebPathResolver _filmwebPathResolver;
    private readonly IFilmwebPostersUrlsRetriever _filmwebHandler;

    public MoviePostersService(ILogger<MoviePostersService> log, IFilmwebPathResolver filmwebPathResolver, IFilmwebPostersUrlsRetriever filmwebHandler)
    {
        _log = log;
        _filmwebPathResolver = filmwebPathResolver;
        _filmwebHandler = filmwebHandler;
    }

    public Task<Maybe<PostersDTO>> GetPosters(Maybe<string> input, CancellationToken cancellationToken) => input.AcceptAsync(GetPosters, _log, cancellationToken);

    private async Task<Maybe<PostersDTO>> GetPosters(string movieUrl, CancellationToken cancellationToken)
    {
        var metadata = _filmwebPathResolver.GetMetadata(movieUrl);
        var posters = await _filmwebHandler.GetPosterUrlsAsync(metadata, cancellationToken);
        var result = new PostersDTO { PosterUrls = posters };
        return new Maybe<PostersDTO>(result, null);
    }
}