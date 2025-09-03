using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Visitors;

internal sealed class GetPostersVisitor : IGetPostersVisitor
{
    private readonly ILogger<GetPostersVisitor> _log;
    private readonly IFilmwebPathResolver _filmwebPathResolver;
    private readonly IFilmwebPostersUrlsRetriever _filmwebHandler;

    public GetPostersVisitor(ILogger<GetPostersVisitor> log, IFilmwebPathResolver filmwebPathResolver, IFilmwebPostersUrlsRetriever filmwebHandler)
    {
        _log = log;
        _filmwebPathResolver = filmwebPathResolver;
        _filmwebHandler = filmwebHandler;
    }

    public async Task<OperationResult<PostersDTO>> SignUp(OperationResult<string> input, CancellationToken cancellationToken)
    {
        var metadata = _filmwebPathResolver.GetMetadata(input.Result!);
        var posters = await _filmwebHandler.GetPosterUrlsAsync(metadata, cancellationToken);
        var result = new PostersDTO { PosterUrls = posters };
        return new OperationResult<PostersDTO>(result, null);
    }

    public ILogger Log => _log;
}