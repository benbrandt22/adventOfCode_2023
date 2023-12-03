using System.Diagnostics;
using System.Text.RegularExpressions;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day03;

public class GearRatios : BaseDayModule
{
    public GearRatios(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 3;
    public override string Title => "Gear Ratios";

    [Fact] public void Part1_Sample() => Part1_PartNumbersTotal("Day03/sample.txt").Should().Be(4361);
    [Fact] public void Part1() => Part1_PartNumbersTotal("Day03/input.txt");

    [Fact] public void Part2_Sample() => Part2_TotalGearRatio("Day03/sample.txt").Should().Be(467835);
    [Fact] public void Part2() => Part2_TotalGearRatio("Day03/input.txt");
    
    public int Part1_PartNumbersTotal(string filename)
    {
        var lines = TextFileLoader.LoadLines(filename).ToList();
        WriteLine($"Loaded Engine {filename} with {lines.Count} lines.");

        var allSymbols = new List<EngineSubstring>();
        var allNumbers = new List<EngineSubstring>();
        for (int rowIndex = 0; rowIndex < lines.Count; rowIndex++)
        {
            var line = lines[rowIndex];
            allNumbers.AddRange(Regex.Matches(line, @"\d+").Select(m => new EngineSubstring(rowIndex, m)));
            allSymbols.AddRange(Regex.Matches(line, @"[^0123456789.]").Select(m => new EngineSubstring(rowIndex, m)));
        }
        WriteLine($"Located {allNumbers.Count} numbers and {allSymbols.Count} symbols");
        
        var partNumbers = allNumbers
            .Where(number => allSymbols.Any(symbol => symbol.IsAdjacentTo(number)))
            .ToList();
        
        WriteLine($"Found {partNumbers.Count} part numbers (with adjacent symbols)");

        var partNumbersTotal = partNumbers.Sum(ppn => int.Parse(ppn.Value));
        WriteLine($"Part Numbers Total: {partNumbersTotal}");
        return partNumbersTotal;
    }
    
    public long Part2_TotalGearRatio(string filename)
    {
        var lines = TextFileLoader.LoadLines(filename).ToList();
        WriteLine($"Loaded Engine {filename} with {lines.Count} lines.");

        var potentialGears = new List<EngineSubstring>();
        var allNumbers = new List<EngineSubstring>();
        for (int rowIndex = 0; rowIndex < lines.Count; rowIndex++)
        {
            var line = lines[rowIndex];
            allNumbers.AddRange(Regex.Matches(line, @"\d+").Select(m => new EngineSubstring(rowIndex, m)));
            potentialGears.AddRange(Regex.Matches(line, @"\*").Select(m => new EngineSubstring(rowIndex, m)));
        }
        WriteLine($"Located {allNumbers.Count} numbers and {potentialGears.Count} potential gears");

        var validGears = potentialGears
            .Select(gear =>
            {
                return new GearAndNumbers(gear, allNumbers.Where(n => n.IsAdjacentTo(gear)).ToList());
            })
            .Where(gn => gn.Numbers.Count == 2) // only gears with 2 adjacent numbers
            .ToList();
        
        WriteLine($"Located {validGears.Count} gears");

        var totalGearRatio = validGears
            .Select(gn =>
            {
                var gear1 = long.Parse(gn.Numbers[0].Value);
                var gear2 = long.Parse(gn.Numbers[1].Value);
                var gearRatio = gear1 * gear2;
                Debug($" {gear1} x {gear2} = {gearRatio}");
                return gearRatio;
            })
            .Sum();
        
        WriteLine($"Total Gear Ratio: {totalGearRatio}");
        return totalGearRatio;
    }

    private record EngineSubstring(string Value, int Row, int[] Columns)
    {
        public EngineSubstring(int row, Match match) : this(match.Value, row, Enumerable.Range(match.Index, match.Value.Length).ToArray() ) { }
        
        public bool IsAdjacentTo(EngineSubstring other)
        {
            bool isValidRow = other.Row <= (Row + 1) && other.Row >= (Row - 1);
            var validColumns = Enumerable.Range(Columns.Min() - 1, Value.Length + 2).ToArray();
            bool hasValidColumn = validColumns.Intersect(other.Columns).Any();
            return isValidRow && hasValidColumn;
        }
    }

    private record GearAndNumbers(EngineSubstring Gear, List<EngineSubstring> Numbers);
}

