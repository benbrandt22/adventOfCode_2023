using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
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

    [Fact] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(1);
    [Fact] public void Part2_Sample2() => ExecutePart2(GetData("sample2")).Should().Be(4);
    [Fact] public void Part2_Sample3() => ExecutePart2(GetData("sample3")).Should().Be(10);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

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
    
    public int ExecutePart2(string data)
    {
        var pipeMap = ParsePipeMap(data);
        WriteLine($"Part 2 - Loaded Pipe Grid with {pipeMap.Rows} rows & {pipeMap.Columns} columns");
        var loop = FindLoop(pipeMap);
        var loopCoordinates = loop.Select(x => new Coordinate(x.Row, x.Column)).ToList();

        bool IsBorder(LinkedGridCell<PipeFitting> cell) => loopCoordinates!.Contains(new Coordinate(cell.Row, cell.Column));
        
        // replace any non-border cell with a "ground" cell
        foreach (var cell in pipeMap.AllCells)
        {
            if (!IsBorder(cell))
            {
                cell.Value = PipeFittingLookup['.'];
            }
        }
        WriteLine(PipeMapAsString(pipeMap, true));

        // Now we have a grid that ONLY includes the full loop. Analyze each point in the grid to see if it's inside or outside the loop
        // Do this by looking at each point and how many times it crosses inside/outside the loop
        
        // define what kind of borders flip the inside/outside state when looking horizontally
        // That is: | vertical pipe, F--7 with 0 or more horizontal pipes between, or L--7 with 0 or more horizontal pipes between
        var borderCrossingsRegex = new Regex(@"\||(F-*J)|(L-*7)");
        
        var insideTileCount = 0;
        foreach (var line in PipeMapAsString(pipeMap).ToLines(true))
        {
            for (int i = 0; i < line.Length; i++)
            {
                bool isBorder = line[i] != '.';
                if (!isBorder)
                {
                    var tilesToTheLeft = line.Substring(0, i);
                    var bordersCrossedToTheLeft = borderCrossingsRegex.Matches(tilesToTheLeft).Count;
                    var isInside = bordersCrossedToTheLeft % 2 == 1; // (crossed an odd number of borders to the left) => inside
                    if (isInside)
                    {
                        insideTileCount++;
                    }
                }
            }
        }

        WriteLine($"Inside tiles: {insideTileCount}");
        return insideTileCount;
    }

    /// <summary>
    /// Finds the cells that make up the loop in the pipe map
    /// </summary>
    /// <param name="pipeMap"></param>
    /// <param name="updateStartCell">Rewrites the start cell with the proper pipe fitting that must exist there</param>
    private List<LinkedGridCell<PipeFitting>> FindLoop(LinkedGrid<PipeFitting> pipeMap, bool updateStartCell = true)
    {
        var startingCell = pipeMap.AllCells.First(cell => cell.Value?.Id == 'S');
        var loopMap = new List<LinkedGridCell<PipeFitting>>();
        loopMap.Add(startingCell);
        
        var currentCell = startingCell;
        var startingDirection = GetStartingDirection(currentCell);
        var startCellDir1 = startingDirection;
        var currentDirection = startingDirection;
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
        var startCellDir2 = FlipDirection(currentDirection);
        
        if (updateStartCell)
        {
            var startPipeFitting = PipeFittingLookup.Values
                .Single(f => (f.End1 == startCellDir1 && f.End2 == startCellDir2) ||
                            (f.End2 == startCellDir1 && f.End1 == startCellDir2));
            startingCell.Value = startPipeFitting;
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
    
    public string PipeMapAsString(LinkedGrid<PipeFitting> pipeMap, bool drawCorners = false)
    {
        var output = new StringBuilder();
        for (int row = 0; row < pipeMap.Rows; row++)
        {
            var rowChars = Enumerable.Range(0, pipeMap.Columns)
                .Select(col => pipeMap[row, col].Value!.Id)
                .ToArray();
            var rowString = new string(rowChars);
            if (drawCorners)
            {
                rowString = rowString
                    .Replace('F', '┌')
                    .Replace('7', '┐')
                    .Replace('J', '┘')
                    .Replace('L', '└');
            }
            output.AppendLine(rowString);
        }
        return output.ToString();
    }
    
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
    
    public record Coordinate(int Row, int Column);
}

