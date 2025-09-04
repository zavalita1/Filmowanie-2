using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Abstractions.Extensions;

public static class MaybeExtensions
{
    public static Maybe<T> Cancelled<T>() => new(default!, new Error("Operation canceled.", ErrorType.Canceled));

    public static Maybe<T> AsMaybe<T>(this T result) => new(result, null);
    public static Maybe<T> AsMaybe<T>(this Error error) => new(default, error);
    public static Maybe<VoidResult> AsVoid<T>(this Maybe<T> maybe) => new(default, maybe.Error);

    public static Maybe<TNext> Map<TNext, TPrevious>(this Maybe<TPrevious> maybe, Func<TPrevious, TNext> selector) =>
        maybe.Error != null 
            ? new (default!, maybe.Error) 
            : new(selector.Invoke(maybe.Result!), null);

    public static Maybe<(T1, T2)> Merge<T1, T2>(this Maybe<T1> first, Maybe<T2> second)
    {
        var error = (Error?)null;

        if (first.Error != null || second.Error != null)
        {
            var firstErrors = first.Error?.ErrorMessages ?? [];
            var secondErrors = second.Error?.ErrorMessages ?? [];
            var errorType = (ErrorType) Math.Max((int)(first.Error?.Type ?? 0), (int)(second.Error?.Type ?? 0));
            var errorMessages = firstErrors.Concat(secondErrors).Distinct();
            error = new Error(errorMessages, errorType);
        }
        
        return new Maybe<(T1, T2)>((first.Result!, second.Result!), error);
    }

    public static Maybe<(T1, T2, T3)> Flatten<T1, T2, T3>(this Maybe<((T1, T2), T3)> operation) => new((operation.Result.Item1.Item1, operation.Result.Item1.Item2, operation.Result.Item2), operation.Error);

    public static async Task<Maybe<TOutput>> AcceptAsync<TInput, TOutput>(this Maybe<TInput> operation, Func<TInput, CancellationToken, Task<Maybe<TOutput>>> func, ILogger log, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return (Cancelled<TOutput>().Merge(operation).Map(x => x.Item1));

        if (operation.Error != null)
            return (new Maybe<TOutput>(default!, operation.Error));

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = await func(operation.Result!, cancellationToken);
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }

    public static async Task<Maybe<TOutput>> AcceptAsync<TOutput>(this Maybe<VoidResult> operation, Func<CancellationToken, Task<Maybe<TOutput>>> func, ILogger log, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return (Cancelled<TOutput>().Merge(operation).Map(x => x.Item1));

        if (operation.Error != null)
            return (new Maybe<TOutput>(default!, operation.Error));

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = await func(cancellationToken);
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }

    public static Maybe<TOutput> Accept<TInput, TOutput>(this Maybe<TInput> operation, Func<TInput, Maybe<TOutput>> func, ILogger log)
    {
        if (operation.Error != null)
            return (new Maybe<TOutput>(default!, operation.Error));

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = func(operation.Result!);
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }

    public static Maybe<TOutput> Accept<TOutput>(this Maybe<VoidResult> operation, Func<Maybe<TOutput>> func, ILogger log)
    {
        if (operation.Error != null)
            return (new Maybe<TOutput>(default!, operation.Error));

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = func();
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }
}