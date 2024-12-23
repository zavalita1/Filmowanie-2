using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface IOperationAsyncVisitor<TInput, TOutput> : IVisitor
{
    public Task<OperationResult<TOutput>> VisitAsync(OperationResult<TInput> input, CancellationToken cancellationToken);
}

public interface IOperationAsyncVisitor<TOutput> : IVisitor
{
    public Task<OperationResult<TOutput>> VisitAsync<T>(OperationResult<T> input, CancellationToken cancellationToken);
}
