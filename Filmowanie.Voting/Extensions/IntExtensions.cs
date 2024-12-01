using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Voting.Extensions;

internal static class IntExtensions
{
    public static Decade ToDecade(this int year) => (Decade)(year - year % 10);
}