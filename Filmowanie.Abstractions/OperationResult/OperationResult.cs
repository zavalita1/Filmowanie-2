namespace Filmowanie.Abstractions.OperationResult;

public readonly record struct OperationResult<T>(T? Result, Error? Error)
{
    public OperationResult(T Result) : this(Result, null)
    { }

    public override string ToString()
    {
        if (Error == null)
            return Result?.ToString() ?? string.Empty;

        return $"Erroneous result ({Error!.Value.Type}).";
    }
};

public readonly record struct VoidResult;