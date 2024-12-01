using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions;

public readonly record struct OperationResult<T>(T? Result, Error? Error)
{
    public override string ToString()
    {
        if (Error != null)
            return Result?.ToString() ?? string.Empty;

        return $"Erroneous result ({Error!.Value.Type}).";
    }
};

public readonly record struct Error(IEnumerable<string> ErrorMessages, ErrorType Type)
{
    public Error(string errorMessage, ErrorType type) : this([errorMessage], type) { }
}

public interface IOperationVisitor<TInput, TOutput>
{
    public OperationResult<TOutput> Visit(OperationResult<TInput> input);
}

public interface IOperationVisitor<TOutput>
{
    public OperationResult<TOutput> Visit<T>(OperationResult<T> input);
}

public interface IOperationAsyncVisitor<TInput, TOutput>
{
    public Task<OperationResult<TOutput>> VisitAsync(OperationResult<TInput> input, CancellationToken cancellationToken);
}

public interface IOperationAsyncVisitor<TOutput>
{
    public Task<OperationResult<TOutput>> VisitAsync<T>(OperationResult<T> input, CancellationToken cancellationToken);
}