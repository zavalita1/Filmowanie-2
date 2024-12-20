namespace Filmowanie.Abstractions.Extensions;

public static class CollectionExtensions
{
    public static string Concat(this IEnumerable<string> collection, string separator) => string.Join(separator, collection);
}