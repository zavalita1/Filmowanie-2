namespace Filmowanie.Abstractions.Extensions;

public static class CollectionExtensions
{
    public static string JoinStrings(this IEnumerable<string> collection, string separator) => string.Join(separator, collection);
    public static string JoinStrings(this IEnumerable<string> collection, char separator) => string.Join(separator, collection);
    public static string JoinStrings(this IEnumerable<string> collection) => collection.JoinStrings(',');
}