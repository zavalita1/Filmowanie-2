using Filmowanie.Abstractions.Enums;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Abstractions.Maybe;

public static class MaybeExtensions
{
    public static T RequireResult<T>(this Maybe<T> maybe)
    {
        if (maybe.Error.HasValue)
            throw new Exception("Required unwrap failed, as there was an error result!");

        return maybe.Result!;
    }

    public static Maybe<T> Cancelled<T>() => new(default!, new Error<T>("Operation canceled.", ErrorType.Canceled));

    public static Maybe<T> AsMaybe<T>(this T result) => new(result, null);
   
    public static Maybe<TNext> Map<TNext, TPrevious>(this Maybe<TPrevious> maybe, Func<TPrevious, TNext> selector) =>
        maybe.Error != null 
            ? new (default!, maybe.Error?.ChangeResultType<TPrevious, TNext>()) 
            : new(selector.Invoke(maybe.Result!), null);

    public static Maybe<(T1, T2, T3)> Merge<T1, T2, T3>(this Maybe<T1> first, Maybe<T2> second, Maybe<T3> third) => first.Merge(second).Merge(third).Flatten();

    public static Maybe<(T1, T2)> Merge<T1, T2>(this Maybe<T1> first, Maybe<T2> second)
    {
        var error = (Error<(T1, T2)>?)null;

        if (first.Error != null || second.Error != null)
        {
            var firstErrors = first.Error?.ErrorMessages ?? [];
            var secondErrors = second.Error?.ErrorMessages ?? [];
            var errorType = (first.Error?.Type ?? ErrorType.None) | (second.Error?.Type ?? ErrorType.None);
            var errorMessages = firstErrors.Concat(secondErrors).Distinct();
            error = new Error<(T1, T2)>(errorMessages, errorType);
        }
        
        return new Maybe<(T1, T2)>((first.Result!, second.Result!), error);
    }

    public static Maybe<T> Merge<T>(this Maybe<T> first, Maybe<VoidResult> second)
    {
        var result = first.Merge<T, VoidResult>(second);
        return new Maybe<T>(result.Result.Item1, result.Error?.ChangeResultType<(T, VoidResult), T>());
    }

    public static Maybe<VoidResult> Merge(this Maybe<VoidResult> first, Maybe<VoidResult> second) => second.Merge<VoidResult>(first);

    public static Maybe<(T1, T2, T3)> Flatten<T1, T2, T3>(this Maybe<((T1, T2), T3)> operation) => new((operation.Result.Item1.Item1, operation.Result.Item1.Item2, operation.Result.Item2), operation.Error?.ChangeResultType<((T1, T2), T3), (T1, T2, T3)>());

    public static async Task<Maybe<TOutput>> AcceptAsync<TInput, TOutput>(this Maybe<TInput> operation, Func<TInput, CancellationToken, Task<Maybe<TOutput>>> func, ILogger log, CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested)
        {
            log.LogWarning("Operation cancelled!");    
            return (Cancelled<TOutput>().Merge(operation).Map(x => x.Item1));
        }

        if (operation.Error != null)
        {
            log.LogWarning("Error occurred! Circuit-breaking the operation..");
            return operation.Error.Value.ChangeResultType<TInput, TOutput>();
        }

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = await func(operation.Result!, cancelToken);
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }

    public static async Task<Maybe<TOutput>> AcceptAsync<TOutput>(this Maybe<VoidResult> operation, Func<CancellationToken, Task<Maybe<TOutput>>> func, ILogger log, CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested)
        {
            log.LogWarning("Operation cancelled!");
            return Cancelled<TOutput>().Merge(operation);
        }

        if (operation.Error != null)
        {
            log.LogWarning("Error occurred! Circuit-breaking the operation..");
            return operation.Error.Value.ChangeResultType<VoidResult, TOutput>();
        }

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = await func(cancelToken);
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }

    public static Maybe<TOutput> Accept<TInput, TOutput>(this Maybe<TInput> operation, Func<TInput, Maybe<TOutput>> func, ILogger log)
    {
        if (operation.Error != null)
        {
            log.LogWarning("Error occurred! Circuit-breaking the operation..");
            return operation.Error.Value.ChangeResultType<TInput, TOutput>();
        }

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = func(operation.Result!);
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }

    public static Maybe<TOutput> Accept<TOutput>(this Maybe<VoidResult> operation, Func<Maybe<TOutput>> func, ILogger log)
    {
        if (operation.Error != null)
        {
            log.LogWarning("Error occurred! Circuit-breaking the operation..");
            return operation.Error.Value.ChangeResultType<VoidResult, TOutput>();
        }

        log.LogDebug($"Starting operation. Using result: {operation}.");
        var result = func();
        log.LogDebug($"Concluded operation. Using result: {operation}.");
        return result;
    }
}