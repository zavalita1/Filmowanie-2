using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions.Extensions;

public static class StringExtensions
{
    public static Decade ToDecade(this string decade)
    {
        if (!decade.EndsWith('s'))
            throw new ArgumentException($"Decade string should end with an 's'. Provided: {decade}!");

        if (!int.TryParse(decade[..^1], out var result))
            throw new ArgumentException($"Not supported value! Provided: {decade}.");

        return (Decade)result;
    }
}