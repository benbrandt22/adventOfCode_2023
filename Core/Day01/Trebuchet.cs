using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day01;

public class Trebuchet : BaseDayModule
{
    public Trebuchet(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 1;
    public override string Title => "Trebuchet?!";

    [Fact] public void Part1Sample() => ProcessPart1(GetData(InputType.Sample)).Should().Be(142);
    [Fact] public void Part1Input() => ProcessPart1(GetData(InputType.Input));
    [Fact] public void Part2Sample() => ProcessPart2(GetData("sample2")).Should().Be(281);
    [Fact] public void Part2Input() => ProcessPart2(GetData(InputType.Input));

    public int ProcessPart1(string data)
    {
        var lines = data.ToLines(removeEmptyLines: true);
        WriteLine($"Loaded data with {lines.Count} lines.");

        var totalDigitsOnlyCalibrationValue = lines
            .Select(line => DigitsOnlyCalibrationValue(line))
            .Sum();

        WriteLine($"Part 1 - Total Digits-Only calibration value: {totalDigitsOnlyCalibrationValue}");
        
        return totalDigitsOnlyCalibrationValue;
    }
    
    private int DigitsOnlyCalibrationValue(string line)
    {
        var digits = line.Where(char.IsDigit).ToList();
        var value = int.Parse($"{digits.First()}{digits.Last()}");
        return value;
    }
    
    public int ProcessPart2(string data)
    {
        var lines = data.ToLines(removeEmptyLines: true);
        WriteLine($"Loaded data with {lines.Count} lines.");

        var totalCalibrationValue = lines
            .Select(line => DigitsOrWordsCalibrationValue(line))
            .Sum();

        WriteLine($"Part 2 - Total Digits/Words calibration value: {totalCalibrationValue}");

        return totalCalibrationValue;
    }
    
    private int DigitsOrWordsCalibrationValue(string line)
    {
        Debug(line);

        var substringsToFind = new[]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"
        };
        
        var foundItems = line.FindSubstrings(substringsToFind, StringComparison.OrdinalIgnoreCase);
        foundItems.ForEach(x => Debug($" ({x.Index} : {x.Value})"));
        
        var digits = foundItems
            .Select(x => x.Value.Length == 1 ? int.Parse(x.Value) : DigitWordToInt(x.Value))
            .ToList();
        
        var value = int.Parse($"{digits.First()}{digits.Last()}");
        
        return value;
    }
    
    private int DigitWordToInt(string digitWord) =>
        digitWord.ToLower() switch
        {
            "zero" => 0,
            "one" => 1,
            "two" => 2,
            "three" => 3,
            "four" => 4,
            "five" => 5,
            "six" => 6,
            "seven" => 7,
            "eight" => 8,
            "nine" => 9,
            _ => throw new ArgumentOutOfRangeException(nameof(digitWord), digitWord, null)
        };
}