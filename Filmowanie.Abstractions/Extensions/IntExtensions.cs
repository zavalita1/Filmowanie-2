using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions.Extensions;

public static class IntExtensions
{
    public static Decade ToDecade(this int year) => (Decade)(year - year % 10);
}