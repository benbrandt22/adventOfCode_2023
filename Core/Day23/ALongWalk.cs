using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day23;

using HikingGraph = Dictionary<ALongWalk.GridCoordinate, HashSet<ALongWalk.GridCoordinate>>;

public class ALongWalk : BaseDayModule
{
    
    public ALongWalk(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 23;
    public override string Title => "A Long Walk";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(94);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(154);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var grid = data.Trim().ToGrid();
        WriteLine($"Part 1 - Loaded Hiking Map with {grid.GetLength(0)} rows and {grid.GetLength(1)} columns");
        var longestPathLength = FindLongestHikingPath(grid);
        WriteLine($"Longest Hiking Path: {longestPathLength}");
        return longestPathLength;
    }

    public long ExecutePart2(string data)
    {
        // Ignore all the slopes by treating them as normal paths. Thus replace all the slopes with '.' and run the same longest path logic as part 1.
        data = data.Replace('^', '.').Replace('v', '.').Replace('<', '.').Replace('>', '.');
        var grid = data.Trim().ToGrid();
        WriteLine($"Part 2 - Loaded (simplified) Hiking Map with {grid.GetLength(0)} rows and {grid.GetLength(1)} columns");
        var longestPathLength = FindLongestHikingPath(grid);
        WriteLine($"Longest Hiking Path: {longestPathLength}");
        return longestPathLength;
    }

    private int FindLongestHikingPath(char[,] grid)
    {
        // in the top row of the grid, find the only cell containing a '.' character
        var startCoordinate = FindFirstLocationOfCharacterInRow(grid, 0, '.');
        WriteLine($"Start Coordinate: {startCoordinate}");
        var endCoordinate = FindFirstLocationOfCharacterInRow(grid, (grid.GetLength(0) - 1), '.');
        WriteLine($"End Coordinate: {endCoordinate}");

        var graph = BuildGraph(grid);
        
        var hikingPaths = new List<HikingPath>() { new() { Path = new() { startCoordinate } } };
        var solvedPaths = new List<HikingPath>();
        
        while (true)
        {
            solvedPaths.AddRange(hikingPaths.Where(hp => hp.LastLocation == endCoordinate));
            
            hikingPaths = hikingPaths
                .Where(hp => hp.LastLocation != endCoordinate)
                .SelectMany(path => EvaluateNext(path, graph)).ToList();
            
            if (hikingPaths.Count == 0) break;
        }
        
        WriteLine($"Found {solvedPaths.Count} solved paths");
        Debug($"Lengths: {solvedPaths.Select(p => p.TotalSteps.ToString()).JoinWith(", ")}");
        
        return solvedPaths.Max(x => x.TotalSteps);
    }

    public List<HikingPath> EvaluateNext(HikingPath path, HikingGraph graph)
    {
        var nextCellOptions = graph[path.LastLocation] // find neighbors from the graph
            .Where(c => !path.Path.Contains(c)).ToHashSet(); // don't revisit cells we've already been to
        var possiblePaths = nextCellOptions.Select(coord =>
        {
            var newPath = path.Copy();
            newPath.Path.Add(coord);
            return newPath;
        }).ToList();
        return possiblePaths;
    }

    public HikingGraph BuildGraph(char[,] grid)
    {
        var graph = new Dictionary<GridCoordinate, HashSet<GridCoordinate>>();
        for (var row = 0; row < grid.GetLength(0); row++)
        {
            for (var col = 0; col < grid.GetLength(1); col++)
            {
                var coord = new GridCoordinate(row, col);
                if (grid[row, col] == '#') continue; // ignore walls
                var neighbors = AvailableNeighbors(grid, coord);
                graph.Add(coord, neighbors.ToHashSet());
            }
        }
        return graph;
    }

    public record HikingGraphNode(GridCoordinate Coordinate, List<HikingGraphNode> NextNodes);
    
    public List<GridCoordinate> AvailableNeighbors(char[,] grid, GridCoordinate currentLocation)
    {
        var possibleMoves = new List<GridCoordinate>();
        
        bool TryMove(GridCoordinate start, Direction direction, char allowedSlope, out GridCoordinate coord)
        {
            var nextLocation = start.Move(direction);
            if (nextLocation.IsInBounds(grid))
            {
                var nextCellValue = grid[nextLocation.Row, nextLocation.Column];
                if (nextCellValue == '.' || nextCellValue == allowedSlope)
                {
                    coord = nextLocation;
                    return true;
                }
            }
            coord = new GridCoordinate(int.MinValue, int.MinValue);
            return false;
        }
        
        if (TryMove(currentLocation, Direction.Up, '^', out var up)) possibleMoves.Add(up);
        if (TryMove(currentLocation, Direction.Down, 'v', out var down)) possibleMoves.Add(down);
        if (TryMove(currentLocation, Direction.Left, '<', out var left)) possibleMoves.Add(left);
        if (TryMove(currentLocation, Direction.Right, '>', out var right)) possibleMoves.Add(right);

        return possibleMoves;
    }

    public class HikingPath
    {
        public List<GridCoordinate> Path { get; set; } = new();
        public int TotalSteps => (Path.Count - 1); // (Starting location doesn't count as a step)
        public GridCoordinate LastLocation => Path.Last();
        public HikingPath Copy()
        {
            // sanity check
            if (Path.Count > 19881) throw new Exception("Path is too long"); // 141*141 = 19881, if we get this length, something is wrong
            return new HikingPath { Path = new List<GridCoordinate>(Path) };
        }
    }
    
    public GridCoordinate FindFirstLocationOfCharacterInRow(char[,] grid, int rowIndex, char targetCharacter)
    {
        return Enumerable.Range(0, grid.GetLength(1))
            .Select(col => (new GridCoordinate(rowIndex, col), grid[rowIndex, col]))
            .First(x => x.Item2 == targetCharacter).Item1;
    }

    public record GridCoordinate(int Row, int Column)
    {
        public GridCoordinate Move(Direction direction, int distance = 1) =>
            new(Row + (direction.DRow * distance), Column + (direction.DCol * distance));
        
        public bool IsInBounds(char[,] grid)
        {
            return Row >= 0 && Row < grid.GetLength(0) && Column >= 0 && Column < grid.GetLength(1);
        }
    }

    public record Direction(int DRow, int DCol)
    {
        public static readonly Direction Up = new(-1, 0);
        public static readonly Direction Down = new(1, 0);
        public static readonly Direction Left = new(0, -1);
        public static readonly Direction Right = new(0, 1);
    }
    
}

