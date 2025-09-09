using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Abstractions.Extensions;

public static class ErrorExtensions
{
    public static Error<TOutput> ChangeResultType<TInput, TOutput>(this Error<TInput> error) => new(error.ErrorMessages, error.Type);
}