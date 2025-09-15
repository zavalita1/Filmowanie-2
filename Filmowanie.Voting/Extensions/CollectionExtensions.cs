namespace Filmowanie.Voting.Extensions;

// TODO UTs
internal static class CollectionExtensions
{
    public static IDictionary<TSelect, int> GetExAeuquoRankings<T, TSortBy, TSelect>(this IEnumerable<T> collection, Func<T, TSortBy> sortByFunc, Func<T, TSelect> selectFunc) where TSelect : notnull
    {
        var result = GetExAeuquoRankingsInner(collection, sortByFunc, selectFunc);
        return result.ToDictionary(x => x.Element, x => x.Place);
    }

    private static IEnumerable<(int Place, TSelect Element)> GetExAeuquoRankingsInner<T, TSortBy, TSelect>(this IEnumerable<T> collection, Func<T, TSortBy> sortByFunc, Func<T, TSelect> selectFunc)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));

        var counter = 0;
        var sorted = collection.OrderBy(sortByFunc).ToArray();

        for (var i = 0; i < sorted.Length; i++)
        {
            if (i == 0 || !sortByFunc(sorted[i - 1])!.Equals(sortByFunc(sorted[i])))
            {
                counter++;
            }

            yield return (counter, selectFunc(sorted[i]));
        }
    }
}