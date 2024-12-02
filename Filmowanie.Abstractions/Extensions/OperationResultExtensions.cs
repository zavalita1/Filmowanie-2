using Filmowanie.Abstractions.Enums;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Abstractions.Extensions;

public static class OperationResultExtensions
{
    public static OperationResult<T> CancelledOperation<T>() => new(default!, new Error("Operation canceled.", ErrorType.Canceled));

    public static OperationResult<TNext> Pluck<TNext, TPrevious>(this OperationResult<TPrevious> operationResult, Func<TPrevious, TNext> selector) =>
        operationResult.Error != null ? new (default!, operationResult.Error) : new(selector.Invoke(operationResult.Result), null);

    public static async Task<OperationResult<TNext>> AcceptAsync<TPrevious, TNext>(this OperationResult<TPrevious> operationResult, Func<TPrevious, CancellationToken, Task<TNext>> inlineVisitor, CancellationToken cancellationToken)
    {
        if (operationResult.Error != null)
            return new OperationResult<TNext>(default!, operationResult.Error);

        var result = await inlineVisitor.Invoke(operationResult.Result, cancellationToken);
        return new OperationResult<TNext>(result, null);
    }

    public static async Task<OperationResult<T>> AcceptAsync<T>(this OperationResult<T> operationResult, Func<T, CancellationToken, Task> inlineVisitor, CancellationToken cancellationToken)
    {
        if (operationResult.Error != null)
            return operationResult with { Result = default! };

        await inlineVisitor.Invoke(operationResult.Result, cancellationToken);
        return operationResult with { Error = null };
    }

    public static OperationResult<object> Empty => new(default!, null);

    public static OperationResult<(T1, T2)> Merge<T1, T2>(this OperationResult<T1> first, OperationResult<T2> second)
    {
        var error = (Error?)null;

        if (first.Error != null || second.Error != null)
        {
            var firstErrors = first.Error?.ErrorMessages ?? Array.Empty<string>();
            var secondErrors = second.Error?.ErrorMessages ?? Array.Empty<string>();
            var errorType = (ErrorType) Math.Max((int)(first.Error?.Type ?? 0), (int)(second.Error?.Type ?? 0));
            error = new Error(firstErrors.Concat(secondErrors), errorType);
        }
        
        return new OperationResult<(T1, T2)>((first.Result, second.Result), error);
    }

    public static OperationResult<(T1, T2, T3)> Flatten<T1, T2, T3>(this OperationResult<((T1, T2), T3)> operation) => new((operation.Result.Item1.Item1, operation.Result.Item1.Item2, operation.Result.Item2), operation.Error);

    public static Task<OperationResult<TOutput>> AcceptAsync<TInput, TOutput>(this OperationResult<TInput> operation, IOperationAsyncVisitor<TInput, TOutput> visitor, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(CancelledOperation<TOutput>().Merge(operation).Pluck(x => x.Item1));

        if (operation.Error != null)
            return Task.FromResult(new OperationResult<TOutput>(default!, operation.Error));

        LogOperation(visitor, operation);
        return visitor.VisitAsync(operation, cancellationToken);
    }

    public static Task<OperationResult<TOutput>> AcceptAsync<TInput, TOutput>(this OperationResult<TInput> operation, IOperationAsyncVisitor<TOutput> visitor, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(CancelledOperation<TOutput>().Merge(operation).Pluck(x => x.Item1));

        if (operation.Error != null)
            return Task.FromResult(new OperationResult<TOutput>(default!, operation.Error));

        LogOperation(visitor, operation);
        return visitor.VisitAsync(operation, cancellationToken);
    }

    public static OperationResult<TOutput> Accept<TInput, TOutput>(this OperationResult<TInput> operation, IOperationVisitor<TInput, TOutput> visitor)
    {
        if (operation.Error != null)
            return new OperationResult<TOutput>(default!, operation.Error);

        LogOperation(visitor, operation);
        return visitor.Visit(operation);
    }

    public static OperationResult<TOutput> Accept<TInput, TOutput>(this OperationResult<TInput> operation, IOperationVisitor<TOutput> visitor)
    {
        if (operation.Error != null)
            return new OperationResult<TOutput>(default!, operation.Error);

        LogOperation(visitor, operation);
        return visitor.Visit(operation);
    }

    private static void LogOperation(IVisitor visitor, object operationResult)
    {
        if (!visitor.Log.IsEnabled(LogLevel.Debug)) 
            return;

        visitor.Log.LogDebug($"Visiting {visitor.GetType().Name} Visitor. Using result: {operationResult}.");
    }
}