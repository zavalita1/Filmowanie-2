namespace Filmowanie.Abstractions.Maybe;

public static class ErrorExtensions
{
    public static Error<TOutput> ChangeResultType<TInput, TOutput>(this Error<TInput> error) => new(error.ErrorMessages, error.Type);
}