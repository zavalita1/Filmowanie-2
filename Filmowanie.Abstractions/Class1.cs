namespace Filmowanie.Abstractions;

public readonly record struct OperationResult<T>(T Result, Error? Error);

public readonly record struct Error(IEnumerable<string> ErrorMessages, ErrorType Type)
{
    public Error(string errorMessage, ErrorType type) : this([errorMessage], type) { }
}

/// <summary>
/// Error codes. Higher error codes have greater priority, i.e. they overtake lesser ones when merging.
/// </summary>
public enum ErrorType 
{
    InvalidState = 100,
    IncomingDataIssue = 200,
    ValidationError = 300,
    Canceled = 400,
    AuthorizationIssue = 500,
}


public static class OperationHelper
{
    public static OperationResult<T> CancelledOperation<T>() => new(default!, new Error("Operation canceled.", ErrorType.Canceled));

    public static OperationResult<TNext> Pluck<TNext, TPrevious>(this OperationResult<TPrevious> operationResult, Func<TPrevious, TNext> selector) =>
        operationResult.Error == null? new (default!, operationResult.Error) : new(selector.Invoke(operationResult.Result), operationResult.Error);

    public static async Task<OperationResult<TNext>> InvokeAsync<TPrevious, TNext>(this OperationResult<TPrevious> operationResult, Func<TPrevious, CancellationToken, Task<TNext>> func, CancellationToken cancellationToken)
    {
        if (operationResult.Error != null)
            return new OperationResult<TNext>(default!, operationResult.Error);

        var result = await func.Invoke(operationResult.Result, cancellationToken);
        return new OperationResult<TNext>(result, null);
    }

    public static async Task<OperationResult<T>> InvokeAsync<T>(this OperationResult<T> operationResult, Func<T, CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        if (operationResult.Error != null)
            return operationResult with { Result = default! };

        await func.Invoke(operationResult.Result, cancellationToken);
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
}

public static class CollectionExtensions
{
    public static string Concat(this IEnumerable<string> collection, char separator) => string.Concat(separator, collection);
}
