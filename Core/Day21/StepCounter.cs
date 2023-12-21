using System.Text;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day21;

public class StepCounter : BaseDayModule
{
    public StepCounter(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 21;
    public override string Title => "Step Counter";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample), 6).Should().Be(16);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input), 64);

    [Fact(Skip = "Not yet implemented")][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data, int stepsToTake)
    {
        var grid = data.Trim().ToGrid();
        WriteLine($"Part 1 - Loaded grid with {grid.GetLength(0)} rows and {grid.GetLength(1)} columns ");

        GridCoordinate startingCoordinate = FindStartCoordinate(grid);
        Debug($"Starting Coordinate: Row {startingCoordinate.Row} Column {startingCoordinate.Column}");
        
        var currentLocations = new HashSet<GridCoordinate>() { startingCoordinate };
        var nextLocations = new HashSet<GridCoordinate>();
        var reachableCountAtThisStep = 0;
        
        for (int step = 0; step < stepsToTake; step++)
        {
            // for each current location, determine possible neighboring cells that can be moved to
            nextLocations.Clear();
            foreach (var currentLocation in currentLocations)
            {
                var neighbors = new List<GridCoordinate> {
                    currentLocation.Move(Up), currentLocation.Move(Down),
                    currentLocation.Move(Left), currentLocation.Move(Right)
                };
                foreach (var neighbor in neighbors)
                {
                    var isInGrid = neighbor.IsInBounds(grid);
                    var isNotRock = grid[neighbor.Row, neighbor.Column] != '#';
                    var isValidMove = isInGrid && isNotRock;
                    
                    if (isValidMove)
                    {
                        nextLocations.Add(neighbor);
                    }
                }
            }
            reachableCountAtThisStep = nextLocations.Count;
            
            // set up current locations for next step analysis
            currentLocations.Clear();
            nextLocations.ToList().ForEach(nl => currentLocations.Add(nl));
        }
        
        WriteLine($"Reachable Garden Plots with {stepsToTake} steps: {reachableCountAtThisStep}");
        return reachableCountAtThisStep;
    }

    private GridCoordinate FindStartCoordinate(char[,] grid)
    {
        // search the grid to find the coordinate of the only 'S' character
        for (var row = 0; row < grid.GetLength(0); row++)
        {
            for (var column = 0; column < grid.GetLength(1); column++)
            {
                if (grid[row, column] == 'S')
                {
                    var startingCoordinate = new GridCoordinate(row, column);
                    return startingCoordinate;
                }
            }
        }
        throw new Exception("Could not find starting coordinate");
    }
    
    public long ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }
    
    public record Direction(int DRow, int DCol);

    public record GridCoordinate(int Row, int Column)
    {
        public GridCoordinate Move(Direction direction, int distance = 1) =>
            new(Row + (direction.DRow * distance), Column + (direction.DCol * distance));
        
        public bool IsInBounds(char[,] grid)
        {
            return Row >= 0 && Row < grid.GetLength(0) && Column >= 0 && Column < grid.GetLength(1);
        }
    }
    
    public static readonly Direction Up = new(-1, 0);
    public static readonly Direction Down = new(1, 0);
    public static readonly Direction Left = new(0, -1);
    public static readonly Direction Right = new(0, 1);

}

