using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions.Maybe;

public readonly record struct Error<T>(IEnumerable<string> ErrorMessages, ErrorType Type)
{
    public Error(string errorMessage, ErrorType type) : this([errorMessage], type) { }

    public static implicit operator Maybe<T>(Error<T> error) => new (default, error);
}