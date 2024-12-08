using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions.Extensions;

public static class StringExtensions
{
    public static Decade ToDecade(this string decade)
    {
        if (!int.TryParse(decade[..^1], out var result))
            throw new ArgumentException($"Not supported value! Provided: {decade}.");

        return (Decade)result;
    }
}