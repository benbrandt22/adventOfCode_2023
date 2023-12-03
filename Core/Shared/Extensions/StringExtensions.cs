namespace Core.Shared.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Joins the collection of strings together with the specified separator.
    /// Acts as a shortcut to string.Join() to allow inline use with a collection of strings.
    /// </summary>
    public static string JoinWith(this IEnumerable<string> values, string separator) => string.Join(separator, values);
    
    /// <summary>
    /// Finds all substrings in a string, returning the index of each match.
    /// </summary>
    public static IEnumerable<int> AllIndexesOf(this string source, string value, StringComparison comparisonType) {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("the string to find may not be empty", nameof(value));
        }

        for (int index = 0;; index += 1) {
            index = source.IndexOf(value, index, comparisonType);
            if (index == -1)
            {
                break;
            }
            yield return index;
        }
    }
    
    /// <summary>
    /// Finds multiple substrings in a string, returning the index and value of each match, in order.
    /// </summary>
    public static List<(string Value, int Index)> FindSubstrings(this string source, string[] substrings, StringComparison comparisonType)
    {
        var results = new List<(string value, int index)>();
        foreach (var substring in substrings)
        {
            var indexes = source.AllIndexesOf(substring, comparisonType);
            foreach (var foundIndex in indexes)
            {
                results.Add((substring, foundIndex));
            }
        }
        return results.OrderBy(x => x.index).ToList();
    }
}