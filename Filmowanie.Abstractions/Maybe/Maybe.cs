using Filmowanie.Abstractions.Extensions;

namespace Filmowanie.Abstractions.Maybe;

public readonly record struct Maybe<T>(T? Result, Error<T>? Error)
{
    internal Maybe(T Result) : this(Result, null)
    { }

    public override string ToString()
    {
        if (Error == null)
            return Result?.ToString() ?? string.Empty;

        return $"Erroneous result ({Error!.Value.Type}).";
    }

    public static implicit operator Maybe<VoidResult> (Maybe<T> result) => new(default, result.Error?.ChangeResultType<T, VoidResult>());
}