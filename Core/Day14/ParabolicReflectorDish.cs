using System.Text.RegularExpressions;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day14;

public class ParabolicReflectorDish : BaseDayModule
{
    public ParabolicReflectorDish(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 14;
    public override string Title => "Parabolic Reflector Dish";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(136);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(64);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var dish = ParseReflectorDish(data);
        WriteLine($"Part 1 - Loaded Reflector Dish with {dish.Rows} rows and {dish.Columns} columns");

        TipDish(dish, Direction.North);
        WriteLine("Tipped North");

        var loadAtNorthSide = GetLoadAtSide(dish, Direction.North);

        WriteLine($"Load at North Side: {loadAtNorthSide}");
        return loadAtNorthSide;
    }

    public long ExecutePart2(string data)
    {
        var dish = ParseReflectorDish(data);
        WriteLine($"Part 1 - Loaded Reflector Dish with {dish.Rows} rows and {dish.Columns} columns");

        var totalCycles = 1000000000;
        
        // Assuming the cycle repeats at some point, find the cycle length
        var sequence = Enumerable.Range(0, totalCycles)
            .Select(i =>
            {
                SpinCycleDish(dish);
                return new ReflectorDishArrangement(dish.Lines);
            });

        var cycleAnalysis = CycleFinder.FindCycle(sequence,
            (x, y) => x.Lines.SequenceEqual(y.Lines),
            totalCycles);
        
        var finalArrangement = cycleAnalysis.FindValueAt(totalCycles - 1);
        
        var loadAtNorthSide = GetLoadAtSide(finalArrangement, Direction.North);
        WriteLine($"Load at North Side: {loadAtNorthSide}");
        return loadAtNorthSide;
    }

    private void SpinCycleDish(ReflectorDishArrangement dish)
    {
        TipDish(dish, Direction.North);
        TipDish(dish, Direction.West);
        TipDish(dish, Direction.South);
        TipDish(dish, Direction.East);
    }

    private ReflectorDishArrangement ParseReflectorDish(string data)
    {
        var lines = data.ToLines(true);
        return new ReflectorDishArrangement(lines);
    }

    public class ReflectorDishArrangement
    {
        public ReflectorDishArrangement(List<string> lines) => Lines = lines;

        public List<string> Lines { get; set; }

        public int Rows => Lines.Count;
        public int Columns => Lines[0].Length;

    }
    
    public enum Direction { North, East, South, West }

    public void TipDish(ReflectorDishArrangement dish, Direction tipDirection)
    {
        switch (tipDirection)
        {
            case Direction.North:
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                TipDishWest(dish);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                break;
            case Direction.East:
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                TipDishWest(dish);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                break;
            case Direction.South:
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                TipDishWest(dish);
                dish.Lines = RotateGridCounterClockwise(dish.Lines);
                break;
            case Direction.West:
                TipDishWest(dish);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tipDirection), tipDirection, null);
        }
    }
    
    public void TipDishWest(ReflectorDishArrangement dish)
    {
        var rollingSectionRegEx = new Regex("[O.]+");
        var newLines = new List<string>();
        for (int i = 0; i < dish.Lines.Count; i++)
        {
            var currentLine = new string(dish.Lines[i].ToArray());
            var rollingSections = rollingSectionRegEx.Matches(currentLine);
            foreach (Match rollingSection in rollingSections)
            {
                var rollingRocksInSection = rollingSection.Value.Count(c => c == 'O');
                var emptySpaceInSection = rollingSection.Value.Length - rollingRocksInSection;
                var newSection = new string('O', rollingRocksInSection) + new string('.', emptySpaceInSection);
                currentLine = currentLine.OverwriteAt(rollingSection.Index, newSection);
            }
            newLines.Add(currentLine);
        }

        dish.Lines = newLines;
    }
    
    private List<string> RotateGridCounterClockwise(List<string> lines)
    {
        var rotatedLines = new List<string>();
        for (int col = lines[0].Length-1; col >= 0; col--)
        {
            var newLine = new string(lines.Select(l => l[col]).ToArray());
            rotatedLines.Add(newLine);
        }

        return rotatedLines;
    }
    
    private long GetLoadAtSide(ReflectorDishArrangement dish, Direction direction)
    {
        var lines = new List<string>(dish.Lines);
        switch (direction)
        {
            case Direction.North:
                lines = RotateGridCounterClockwise(dish.Lines);
                return GetLoadAtWestSide(lines);
            case Direction.East:
                lines = RotateGridCounterClockwise(dish.Lines);
                lines = RotateGridCounterClockwise(dish.Lines);
                return GetLoadAtWestSide(lines);
            case Direction.South:
                lines = RotateGridCounterClockwise(dish.Lines);
                lines = RotateGridCounterClockwise(dish.Lines);
                lines = RotateGridCounterClockwise(dish.Lines);
                return GetLoadAtWestSide(lines);
            case Direction.West:
                return GetLoadAtWestSide(lines);
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
    
    private int GetLoadAtWestSide(List<string> lines)
    {
        var width = lines[0].Length;
        var totalLoad = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            var rollingRockIndexes = lines[i].AllIndexesOf("O", StringComparison.Ordinal);
            var lineLoad = rollingRockIndexes.Sum(rollingRockIndex => width - rollingRockIndex);
            totalLoad += lineLoad;
        }

        return totalLoad;
    }
}

