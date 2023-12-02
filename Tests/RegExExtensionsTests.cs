using System.Text.RegularExpressions;
using Core.Shared.Extensions;

namespace Tests;

public class RegExExtensionsTests
{
    [Theory]
    [ClassData(typeof(MapToTestData))]
    public void MapTo_MapsMatchToObject_AsExpected(string input, Regex pattern, Type expectedType, object expectedResult)
    {
        var match = pattern.Matches(input).First();
        
        var mapToMethodInfo = typeof(RegExExtensions).GetMethod(nameof(RegExExtensions.MapTo), new[] { typeof(Match) });
        var mapToGenericMethod = mapToMethodInfo!.MakeGenericMethod(expectedType);
        var result = mapToGenericMethod.Invoke(null, new[] { match });
        
        result.Should().BeEquivalentTo(expectedResult);
    }
}

public class MapToTestData : TheoryData<string, Regex, Type, object>
{
    public record StringIntegerDecimalRecord(string String, int Integer, decimal Decimal);
    
    public class StringIntegerDecimalClass
    {
        public string String { get; set; }
        public int Integer { get; set; }
        public decimal Decimal { get; set; }
    }
    
    public class StringAndListClass
    {
        public string String { get; set; }
        public List<int> Integers { get; set; } = new();
    }
    
    public MapToTestData()
    {
        Add(" MyText 123 1.23 ",
            new Regex(@"(?<string>\w+)\s+(?<integer>\d+)\s+(?<decimal>\d+\.\d+)"),
            typeof(StringIntegerDecimalRecord),
            new StringIntegerDecimalRecord("MyText", 123, 1.23m));

        Add(" MyText 123 1.23 ",
            new Regex(@"(?<string>\w+)\s+(?<integer>\d+)\s+(?<decimal>\d+\.\d+)"),
            typeof(StringIntegerDecimalClass),
            new StringIntegerDecimalClass() { String = "MyText", Integer = 123, Decimal = 1.23m });

        // Only set properties found in the regex match, ignore others
        Add("Name: John",
            new Regex(@"Name:\s+(?<string>\w+)"),
            typeof(StringAndListClass),
            new StringAndListClass() { String = "John", Integers = new() });
    }
}