using Core.Shared.Extensions;

namespace Tests;

public class StringExtensionsTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData("abc", "a", new[] { 0 })]
    [InlineData("abc", "b", new[] { 1 })]
    [InlineData("abc", "c", new[] { 2 })]
    [InlineData("abc", "d", new int[] { })]
    [InlineData("123123", "23", new[] { 1, 4 })]
    [InlineData("xx111yy111zz", "111", new[] { 2, 7 })]
    [InlineData("xx111yy111zz", "11", new[] { 2, 3, 7, 8 })]
    public void AllIndexesOf_Returns_Expected_Results(string source, string value, int[] expectedIndexes)
    {
        outputHelper.WriteLine($"source: {source} value: {value}");
        var indexes = source.AllIndexesOf(value, StringComparison.OrdinalIgnoreCase);
        indexes.Should().BeEquivalentTo(expectedIndexes);
    }
    
    [Theory]
    [ClassData(typeof(FindSubstringsTestCases))]
    public void FindSubstrings_Returns_Expected_Results(string source, string[] substrings, (string Value, int Index)[] expectedResults)
    {
        outputHelper.WriteLine($"source: {source} substrings: {string.Join(", ", substrings)}");
        var results = source.FindSubstrings(substrings, StringComparison.OrdinalIgnoreCase);
        results.Should().BeEquivalentTo(expectedResults, opt => opt.WithStrictOrdering());
    }

    private class FindSubstringsTestCases : TheoryData<string, string[], (string Value, int Index)[]>
    {
        public FindSubstringsTestCases()
        {
            Add("abc", new[] { "a" }, new[] { ("a", 0) });
            Add("abcabc", new[] { "a" }, new[] { ("a", 0), ("a", 3) });
            Add("abcabc", new[] { "a", "b" }, new[] { ("a", 0), ("b", 1), ("a", 3), ("b", 4) });
            Add("abcabc", new[] { "ab", "abc" }, new[] { ("ab", 0), ("abc", 0), ("ab", 3), ("abc", 3) });
        }
    }
}