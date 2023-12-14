using Core.Shared.Extensions;

namespace Tests;

public class StringExtensionsTests(ITestOutputHelper outputHelper)
{
    private string WriteOutEscapedLinefeeds(string input)
    {
        return input.Replace("\r", "\\r").Replace("\n", "\\n");
    }
    
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
    
    [Theory]
    [InlineData(new[] { "a", "b", "c" }, ", ", "a, b, c")]
    [InlineData(new[] { "a", "b", "c" }, " ", "a b c")]
    [InlineData(new[] { "a", "b", "c" }, "", "abc")]
    [InlineData(new[] { "a", "b", "c" }, "123", "a123b123c")]
    [InlineData(new[] { "apple", "orange", "banana" }, "|", "apple|orange|banana")]
    [InlineData(new string[]{}, ",", "")]
    public void JoinWith_Returns_Expected_Results(string[] values, string separator, string expected)
    {
        outputHelper.WriteLine($"values: {string.Join(", ", values)} separator: {separator}");
        var result = values.JoinWith(separator);
        result.Should().Be(expected);
    }
    
    [Theory]
    [InlineData("abc", new[] { "abc" })]
    [InlineData("abc\r\n\r\n123", new[] { "abc", "123" })]
    [InlineData("abc\r\n123", new[] { "abc\r\n123" })]
    [InlineData("abc\n\n123", new[] { "abc", "123" })]
    [InlineData("abc\n123", new[] { "abc\n123" })]
    [InlineData("Section\r\nOne\r\n\r\nSection\r\nTwo", new[] { "Section\r\nOne", "Section\r\nTwo" })]
    public void ToParagraphs_Returns_Expected_Results(string input, string[] expected)
    {
        outputHelper.WriteLine($"input:\r\n{WriteOutEscapedLinefeeds(input)}");
        var result = input.ToParagraphs();
        result.Should().BeEquivalentTo(expected);
    }
    
    [Theory]
    [InlineData("abc", new[] { "abc" })]
    [InlineData("abc\r\n123", new[] { "abc", "123" })]
    [InlineData("abc\n123", new[] { "abc", "123" })]
    [InlineData("abc\r\n\r\n123", new[] { "abc", "", "123" })]
    [InlineData("abc\n\n123", new[] { "abc", "", "123" })]
    [InlineData("abc\n123\n", new[] { "abc", "123", "" })]
    public void ToLines_Returns_Expected_Results(string input, string[] expected)
    {
        outputHelper.WriteLine($"input:\r\n{WriteOutEscapedLinefeeds(input)}");
        var result = input.ToLines();
        result.Should().BeEquivalentTo(expected);
    }
    
    [Theory]
    [InlineData("abc", new[] { "abc" })]
    [InlineData("abc\r\n123", new[] { "abc", "123" })]
    [InlineData("abc\n123", new[] { "abc", "123" })]
    [InlineData("abc\r\n\r\n123", new[] { "abc", "123" })]
    [InlineData("abc\n\n123", new[] { "abc", "123" })]
    [InlineData("abc\n123\n", new[] { "abc", "123" })]
    public void ToLines_With_RemoveEmptyLines_Returns_Expected_Results(string input, string[] expected)
    {
        outputHelper.WriteLine($"input:\r\n{WriteOutEscapedLinefeeds(input)}");
        var result = input.ToLines(true);
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("abc", 0, 'x', "xbc")]
    [InlineData("abc", 1, 'x', "axc")]
    [InlineData("abc", 2, 'x', "abx")]
    public void ReplaceAt_Returns_Expected_Results(string input, int index, char newChar, string expected)
    {
        outputHelper.WriteLine($"input: \"{input}\" index: {index} newChar: {newChar}");
        var result = input.ReplaceAt(index, newChar);
        result.Should().Be(expected);
    }
    
    [Fact]
    public void ReplaceAt_With_Index_Out_Of_Range_Throws_ArgumentOutOfRangeException()
    {
        Action indexLow = () => "abc".ReplaceAt(-1, 'x');
        indexLow.Should().Throw<IndexOutOfRangeException>();
        
        Action indexHigh = () => "abc".ReplaceAt(3, 'x');
        indexHigh.Should().Throw<IndexOutOfRangeException>();
    }
    
    [Fact]
    public void ReplaceAt_With_Empty_Input_Throws_ArgumentException()
    {
        Action nullInput = () => string.Empty.ReplaceAt(0, 'x');
        nullInput.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void ReplaceAt_With_Null_Input_Throws_ArgumentNullException()
    {
        Action nullInput = () => ((string)null!).ReplaceAt(0, 'x');
        nullInput.Should().Throw<ArgumentNullException>();
    }
    
    [Theory]
    [InlineData("abc", 0, "x", "xbc")]
    [InlineData("abc", 1, "x", "axc")]
    [InlineData("abc", 2, "x", "abx")]
    [InlineData("abc", 0, "xyz", "xyz")]
    [InlineData("12345", 0, "abc", "abc45")]
    [InlineData("12345", 1, "abc", "1abc5")]
    [InlineData("12345", 2, "abc", "12abc")]
    public void OverwriteAt_Returns_Expected_Results(string input, int index, string newSubstring, string expected)
    {
        outputHelper.WriteLine($"input: \"{input}\" index: {index} newSubstring: \"{newSubstring}\"");
        var result = input.OverwriteAt(index, newSubstring);
        result.Should().Be(expected);
    }

}