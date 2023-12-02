using System.Text.RegularExpressions;
using Core.Shared.Extensions;

namespace Tests;

public class RegExExtensionsTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [ClassData(typeof(MapToTestData))]
    public void MapTo_MapsMatchToObject_AsExpected(string input, Regex pattern, Type expectedType, object expectedResult)
    {
        outputHelper.WriteLine($"Input: {input}");
        outputHelper.WriteLine($"Pattern: {pattern}");
        outputHelper.WriteLine($"Expected type: {expectedType}");
        
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
    
    public record EnumAndIntClass(DayOfWeek DayOfWeek, int DayOfMonth) { }
    
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
        
        // verify we can map a string regex match to an Enum
        Add("DOW: saturday, DOM: 2",
            new Regex(@"DOW:\s+(?<dayOfWeek>\w+),\s+DOM:\s+(?<dayOfMonth>\d+)"),
            typeof(EnumAndIntClass),
            new EnumAndIntClass(DayOfWeek.Saturday, 2));
    }
}