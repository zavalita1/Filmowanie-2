namespace Filmowanie.Abstractions.OperationResult;

public readonly record struct Maybe<T>(T? Result, Error? Error)
{
    public Maybe(T Result) : this(Result, null)
    { }

    public override string ToString()
    {
        if (Error == null)
            return Result?.ToString() ?? string.Empty;

        return $"Erroneous result ({Error!.Value.Type}).";
    }
};

public readonly record struct VoidResult
{
    public static Maybe<VoidResult> Void => new(new (), null);
}