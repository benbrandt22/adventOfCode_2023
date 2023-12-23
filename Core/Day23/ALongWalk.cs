using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day23;

using HikingGraph = Dictionary<ALongWalk.GridCoordinate, HashSet<(ALongWalk.GridCoordinate neighbor, int distance)>>;

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
        var longestPathLength = FindLongestHikingPath(grid, simplifyGraph: true);
        WriteLine($"Longest Hiking Path: {longestPathLength}");
        return longestPathLength;
    }

    private int FindLongestHikingPath(char[,] grid, bool simplifyGraph = false)
    {
        // in the top row of the grid, find the only cell containing a '.' character
        var startCoordinate = FindFirstLocationOfCharacterInRow(grid, 0, '.');
        WriteLine($"Start Coordinate: {startCoordinate}");
        var endCoordinate = FindFirstLocationOfCharacterInRow(grid, (grid.GetLength(0) - 1), '.');
        WriteLine($"End Coordinate: {endCoordinate}");

        var graph = BuildGraph(grid);
        if (simplifyGraph)
        {
            // simplify any direct paths to minimize nodes
            SimplifyUndirectedGraph(graph);
        }

        var hikingPaths = new List<HikingPath>(){new(new List<GridCoordinate>(){startCoordinate}, 0)};
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
            .Where(c => !path.HasVisited(c.neighbor)).ToHashSet(); // don't revisit cells we've already been to
        var possiblePaths = nextCellOptions.Select(neighbor =>
        {
            var newPath = path.Copy();
            newPath.Add(neighbor.neighbor, neighbor.distance);
            return newPath;
        }).ToList();
        return possiblePaths;
    }

    public HikingGraph BuildGraph(char[,] grid)
    {
        var graph = new Dictionary<GridCoordinate, HashSet<(GridCoordinate neighbor, int distance)>>();
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
    
    public void SimplifyUndirectedGraph(HikingGraph graph)
    {
        // Any node with two neighbors can be removed from the graph and replaced with a single edge between the two neighbors
        while (true)
        {
            var nodesToRemove = graph.Where(n => n.Value.Count == 2).Take(1).ToList();
            if(!nodesToRemove.Any()) break;
            var coord = nodesToRemove.Single().Key;
            var neighbors = nodesToRemove.Single().Value;
            
            var (neighbor1, distance1) = neighbors.First();
            var (neighbor2, distance2) = neighbors.Last();
            graph.Remove(coord);
            graph[neighbor1].Remove((coord, distance1));
            graph[neighbor2].Remove((coord, distance2));
            graph[neighbor1].Add((neighbor2, distance1 + distance2));
            graph[neighbor2].Add((neighbor1, distance1 + distance2));
        }
    }
    
    public List<(GridCoordinate coordinate, int distance)> AvailableNeighbors(char[,] grid, GridCoordinate currentLocation)
    {
        var possibleMoves = new List<(GridCoordinate coordinate, int distance)>();
        
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

        if (TryMove(currentLocation, Direction.Up, '^', out var up)) possibleMoves.Add((up, 1));
        if (TryMove(currentLocation, Direction.Down, 'v', out var down)) possibleMoves.Add((down, 1));
        if (TryMove(currentLocation, Direction.Left, '<', out var left)) possibleMoves.Add((left, 1));
        if (TryMove(currentLocation, Direction.Right, '>', out var right)) possibleMoves.Add((right, 1));

        return possibleMoves;
    }

    public class HikingPath
    {
        public HikingPath(List<GridCoordinate> path, int totalSteps = 0)
        {
            Path = path;
            TotalSteps = totalSteps;
        }

        public List<GridCoordinate> Path { get; private set; }
        public int TotalSteps { get; private set; }
        public GridCoordinate LastLocation => Path.Last();
        public HikingPath Copy()
        {
            return new HikingPath(new List<GridCoordinate>(Path), TotalSteps);
        }

        public bool HasVisited(GridCoordinate neighbor)
        {
            return Path.Contains(neighbor);
        }

        public void Add(GridCoordinate location, int distance)
        {
            Path.Add(location);
            TotalSteps += distance;
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

