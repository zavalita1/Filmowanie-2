using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Outgoing;

namespace Filmowanie.Nomination.Interfaces;

internal interface IMoviePostersService
{
    Task<Maybe<PostersDTO>> GetPosters(Maybe<string> input, CancellationToken cancellationToken);
}