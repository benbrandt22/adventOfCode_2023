using System.Diagnostics;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day10;

public class PipeMaze : BaseDayModule
{
    public PipeMaze(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 10;
    public override string Title => "Pipe Maze";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(8);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not yet implemented")] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public int ExecutePart1(string data)
    {
        var pipeMap = ParsePipeMap(data);
        WriteLine($"Part 1 - Loaded Pipe Grid with {pipeMap.Rows} rows & {pipeMap.Columns} columns");
        
        var loopMap = FindLoop(pipeMap);
        
        WriteLine($"Loop mapped out with {loopMap.Count} cells");
        var loopAsString = new string(loopMap.Select(x => x.Value.Id).ToArray());
        WriteLine(loopAsString);
        
        var midpointDistanceFromStart = loopMap.Count / 2;
        WriteLine($"Loop Midpoint Distance From Start: {midpointDistanceFromStart}");
        return midpointDistanceFromStart;
    }

    private List<LinkedGridCell<PipeFitting>> FindLoop(LinkedGrid<PipeFitting> pipeMap)
    {
        var startingCell = pipeMap.AllCells.First(cell => cell.Value?.Id == 'S');
        var loopMap = new List<LinkedGridCell<PipeFitting>>();
        loopMap.Add(startingCell);
        
        var currentCell = startingCell;
        var currentDirection = GetStartingDirection(currentCell);
        // move off the starting cell and start mapping the pipe fittings
        currentCell = currentCell.GetNeighbor(ToGridDirection(currentDirection))!;
        while (true)
        {
            loopMap.Add(currentCell);
            var enteredSide = FlipDirection(currentDirection);
            var nextDirection = currentCell.Value!.ConnectsTo(enteredSide);
            if (nextDirection == null)
            {
                break;
            }
            currentCell = currentCell.GetNeighbor(ToGridDirection(nextDirection.Value))!;
            currentDirection = nextDirection.Value;
            if (currentCell.Value!.Id == 'S') { break; }
        }

        return loopMap;
    }

    private Direction FlipDirection(Direction currentDirection)
    {
        return currentDirection switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => throw new ArgumentOutOfRangeException(nameof(currentDirection), currentDirection, null)
        };
    }

    private Direction GetStartingDirection(LinkedGridCell<PipeFitting> startingCell)
    {
        if (startingCell.NeighborUp != null && startingCell.NeighborUp.Value!.CanEnterAt(Direction.South)) { return Direction.North; }
        if (startingCell.NeighborRight != null && startingCell.NeighborRight.Value!.CanEnterAt(Direction.West)) { return Direction.East; }
        if (startingCell.NeighborDown != null && startingCell.NeighborDown.Value!.CanEnterAt(Direction.North)) { return Direction.South; }
        if (startingCell.NeighborLeft != null && startingCell.NeighborLeft.Value!.CanEnterAt(Direction.East)) { return Direction.West; }
        throw new Exception("Could not find starting direction");
    }

    public int ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }

    private LinkedGrid<PipeFitting> ParsePipeMap(string data)
    {
        var lines = data.ToLines(true);
        var totalRows = lines.Count;
        var totalColumns = lines[0].Length;
        var grid = new LinkedGrid<PipeFitting>(totalRows, totalColumns);

        for (int r = 0; r < lines.Count; r++)
        {
            var line = lines[r];
            for (int c = 0; c < line.Length; c++)
            {
                grid[r, c].Value = PipeFittingLookup[line[c]];
            }
        }

        return grid;
    }

    public record PipeFitting(char Id, Direction? End1, Direction? End2)
    {
        public Direction? ConnectsTo(Direction inDirection)
        {
            Direction? outDirection = null;
            if(End1 == inDirection) { outDirection = End2; }
            else if(End2 == inDirection) { outDirection = End1; }
            return outDirection;
        }

        public bool CanEnterAt(Direction side)
        {
            return End1 == side || End2 == side;
        }
    }

    public static Dictionary<char, PipeFitting> PipeFittingLookup = new Dictionary<char, PipeFitting>()
    {
        { '.', new PipeFitting('.', null, null)}, // no fitting
        { 'S', new PipeFitting('S', null, null)}, // start position
        { '|', new PipeFitting('|', Direction.North, Direction.South)},
        { '-', new PipeFitting('-', Direction.East, Direction.West)},
        { 'L', new PipeFitting('L', Direction.North, Direction.East)},
        { 'J', new PipeFitting('J', Direction.North, Direction.West)},
        { '7', new PipeFitting('7', Direction.South, Direction.West)},
        { 'F', new PipeFitting('F', Direction.South, Direction.East)}
    };
    
    public enum Direction { North, East, South, West }
    
    public LinkedGridDirection ToGridDirection(Direction direction)
    {
        return direction switch
        {
            Direction.North => LinkedGridDirection.Up,
            Direction.East => LinkedGridDirection.Right,
            Direction.South => LinkedGridDirection.Down,
            Direction.West => LinkedGridDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}

