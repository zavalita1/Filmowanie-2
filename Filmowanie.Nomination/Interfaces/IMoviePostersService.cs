using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Outgoing;

namespace Filmowanie.Nomination.Interfaces;

internal interface IMoviePostersService
{
    Task<OperationResult<PostersDTO>> GetPosters(OperationResult<string> input, CancellationToken cancellationToken);
}