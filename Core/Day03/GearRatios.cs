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
        // TODO: try refactoring to use new EngineSubstring Adjacent logic I developed for Part 2
        var lines = TextFileLoader.LoadLines(filename).ToList();
        WriteLine($"Loaded Engine {filename} with {lines.Count} lines.");

        var allPotentialPartNumbers = new List<PotentialPartNumber>();
        for (int rowIndex = 0; rowIndex < lines.Count; rowIndex++)
        {
            var line = lines[rowIndex];
            var potentialNumbersInLine = Regex.Matches(line, @"\d+")
                .Select(m => new PotentialPartNumber(int.Parse(m.Value), rowIndex, m.Index));
            allPotentialPartNumbers.AddRange(potentialNumbersInLine);
        }
        WriteLine($"Found {allPotentialPartNumbers.Count} potential part numbers");
        
        var engineBounds = new CoordRect(0, lines.Count - 1, 0, lines[0].Length - 1);

        var partNumbers = allPotentialPartNumbers
            .Where(ppn =>
            {
                var surroundingRect = new CoordRect(ppn.Row - 1, ppn.Row + 1, ppn.Column - 1, ppn.Column + ppn.Value.ToString().Length)
                    .BindWithin(engineBounds);
                var surroundingSymbols = surroundingRect.ToCoordinates()
                    .Select(c => lines[c.Row][c.Column])
                    .Where(c => !char.IsDigit(c) && c != '.')
                    .ToList();
                Debug($" {ppn.Value} (Row {ppn.Row}, Column {ppn.Column}) - Surrounding Symbols: {new string(surroundingSymbols.ToArray())}");
                // if any surrounding symbols, then its a part number
                return surroundingSymbols.Any();
            })
            .ToList();
        
        WriteLine($"Found {partNumbers.Count} part numbers (with adjacent symbols)");
        
        var partNumbersTotal = partNumbers.Sum(ppn => ppn.Value);
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

    private record PotentialPartNumber(int Value, int Row, int Column);
    private record Coordinate(int Row, int Column);

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

    private record CoordRect(int RowMin, int RowMax, int ColumnMin, int ColumnMax)
    {
        public CoordRect BindWithin(CoordRect outerBounds)
        {
            var rowMin = Math.Max(outerBounds.RowMin, RowMin);
            var rowMax = Math.Min(outerBounds.RowMax, RowMax);
            var columnMin = Math.Max(outerBounds.ColumnMin, ColumnMin);
            var columnMax = Math.Min(outerBounds.ColumnMax, ColumnMax);
            return new CoordRect(rowMin, rowMax, columnMin, columnMax);
        }
        
        public List<Coordinate> ToCoordinates()
        {
            var coordinates = new List<Coordinate>();
            for (int row = RowMin; row <= RowMax; row++)
            {
                for (int column = ColumnMin; column <= ColumnMax; column++)
                {
                    coordinates.Add(new Coordinate(row, column));
                }
            }
            return coordinates;
        }
    }
}

