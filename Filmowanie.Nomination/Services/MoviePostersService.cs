using Filmowanie.Abstractions.Maybe;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Services;

// TODO UTs
internal sealed class MoviePostersService : IMoviePostersService
{
    private readonly ILogger<MoviePostersService> log;
    private readonly IFilmwebPathResolver filmwebPathResolver;
    private readonly IFilmwebPostersUrlsRetriever filmwebHandler;

    public MoviePostersService(ILogger<MoviePostersService> log, IFilmwebPathResolver filmwebPathResolver, IFilmwebPostersUrlsRetriever filmwebHandler)
    {
        this.log = log;
        this.filmwebPathResolver = filmwebPathResolver;
        this.filmwebHandler = filmwebHandler;
    }

    public Task<Maybe<PostersDTO>> GetPosters(Maybe<string> input, CancellationToken cancelToken) => input.AcceptAsync(GetPosters, this.log, cancelToken);

    private async Task<Maybe<PostersDTO>> GetPosters(string movieUrl, CancellationToken cancelToken)
    {
        var metadata = this.filmwebPathResolver.GetMetadata(movieUrl);
        var posters = await this.filmwebHandler.GetPosterUrlsAsync(metadata, cancelToken);
        var result = new PostersDTO { PosterUrls = posters };
        return new Maybe<PostersDTO>(result, null);
    }
}