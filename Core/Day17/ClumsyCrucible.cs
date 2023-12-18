using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day17;

public class ClumsyCrucible : BaseDayModule
{
    public ClumsyCrucible(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 17;
    public override string Title => "Clumsy Crucible";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(102);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(94);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var grid = data.Trim().ToIntegerGrid();
        WriteLine($"Part 1 - Loaded grid with {grid.GetLength(0)} rows and {grid.GetLength(1)} columns ");

        var solution = Solve(grid, 0, 3);
        WriteLine($"Solution: {solution}");
        return solution;
    }
    
    public long ExecutePart2(string data)
    {
        var grid = data.Trim().ToIntegerGrid();
        WriteLine($"Part 2 - Loaded grid with {grid.GetLength(0)} rows and {grid.GetLength(1)} columns ");

        throw new NotImplementedException("Passes with the sample, but fails with the input. Need to figure out why.");
        
        var solution = Solve(grid, 4, 10);
        WriteLine($"Solution: {solution}");
        return solution;
    }

    public int Solve(int[,] grid, int minSteps, int maxSteps)
    {
        // Learning path finding using code adapted from https://github.com/stevehjohn/AoC/blob/master/AoC.Solutions/Solutions/2023/17/Base.cs
        var destination = new GridCoordinate(grid.GetLength(0) - 1, grid.GetLength(1) - 1);
        
        var queue = new PriorityQueue<(GridCoordinate Coordinate, Direction Direction, int Steps), int>();

        var visited = new HashSet<string>();
        
        queue.Enqueue((new(0,0), East, 1), 0);
        queue.Enqueue((new(0,0), South, 1), 0);

        var directions = new List<Direction>();
        
        while (queue.TryDequeue(out var item, out var cost))
        {
            if (item.Coordinate == destination && item.Steps >= minSteps - 1)
            {
                return cost;
            }

            directions.Clear();
            
            if (item.Steps < minSteps - 1)
            {
                directions.Add(item.Direction);
            }
            else
            {
                GetDirections(item.Direction, directions);
            }

            foreach (var direction in directions)
            {
                var newSteps = direction == item.Direction ? item.Steps + 1 : 0;

                if (newSteps == maxSteps)
                {
                    continue;
                }

                var nextCoordinate = item.Coordinate.Move(direction);

                if (!nextCoordinate.IsInBounds(grid))
                {
                    continue;
                }
                
                var key = $"{item.Coordinate.Row},{item.Coordinate.Column},{nextCoordinate.Row},{nextCoordinate.Column},{newSteps}";
                
                if (visited.Add(key))
                {
                    queue.Enqueue((nextCoordinate, direction, newSteps), cost + grid[nextCoordinate.Row, nextCoordinate.Column]);
                }
            }
        }

        return 0;
    }
    
    private static void GetDirections(Direction direction, List<Direction> directions)
    {
        if (direction != South) { directions.Add(North); }
        if (direction != West) { directions.Add(East); }
        if (direction != North) { directions.Add(South); }
        if (direction != East) { directions.Add(West); }
    }
    
    public static readonly Direction North = new(-1, 0);
    public static readonly Direction South = new(1, 0);
    public static readonly Direction East = new(0, 1);
    public static readonly Direction West = new(0, -1);
    
    public record Direction(int DRow, int DCol);

    public record GridCoordinate(int Row, int Column)
    {
        public bool IsInBounds(int[,] grid) =>
            Row >= 0 && Row < grid.GetLength(0) && Column >= 0 && Column < grid.GetLength(1);

        public GridCoordinate Move(Direction direction) =>
            new(Row + direction.DRow, Column + direction.DCol);
    }

}

