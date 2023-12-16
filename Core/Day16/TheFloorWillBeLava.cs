using System.Text;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day16;

public class TheFloorWillBeLava : BaseDayModule
{
    public TheFloorWillBeLava(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 16;
    public override string Title => "The Floor Will Be Lava";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(46);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(51);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var grid = data.Trim().ToGrid();
        WriteLine($"Part 1 - Loaded grid with {grid.GetLength(0)} rows and {grid.GetLength(1)} columns ");

        var totalEnergizedTiles = GetTotalEnergizedTiles(grid, new NextStep(new GridCoordinate(0, 0), Direction.Right));
        WriteLine($"Total Energized Tiles: {totalEnergizedTiles}");
        return totalEnergizedTiles;
    }
    
    public long ExecutePart2(string data)
    {
        var grid = data.Trim().ToGrid();
        WriteLine($"Part 2 - Loaded grid with {grid.GetLength(0)} rows and {grid.GetLength(1)} columns ");
        
        var allPotentialStartingSteps = new List<NextStep>();
        for (var row = 0; row < grid.GetLength(0); row++)
        {
            // add starts from the left and right edges
            allPotentialStartingSteps.Add(new NextStep(new GridCoordinate(row, 0), Direction.Right));
            allPotentialStartingSteps.Add(new NextStep(new GridCoordinate(row, grid.GetLength(1) - 1), Direction.Left));
        }
        for (var column = 0; column < grid.GetLength(1); column++)
        {
            // add starts from the top and bottom edges
            allPotentialStartingSteps.Add(new NextStep(new GridCoordinate(0, column), Direction.Down));
            allPotentialStartingSteps.Add(new NextStep(new GridCoordinate(grid.GetLength(0) - 1, column), Direction.Up));
        }
        
        var optimalStartAndTotalEnergized = allPotentialStartingSteps
            .Select(s => (start: s, energized: GetTotalEnergizedTiles(grid, s)))
            .MaxBy(x => x.energized);

        WriteLine($"Optimal Starting Location: Row {optimalStartAndTotalEnergized.start.Coordinate.Row}, Column {optimalStartAndTotalEnergized.start.Coordinate.Column}, Direction {optimalStartAndTotalEnergized.start.Direction}");
        WriteLine($"Total Energized Tiles: {optimalStartAndTotalEnergized.energized}");
        return optimalStartAndTotalEnergized.energized;
    }
    
    public long GetTotalEnergizedTiles(char[,] grid, NextStep firstStep)
    {
        WriteLine($" Checking: Row {firstStep.Coordinate.Row}, Column {firstStep.Coordinate.Column}, Direction {firstStep.Direction}");

        var energizedTiles = new HashSet<GridCoordinate>();
        var seenSteps = new HashSet<NextStep>();
        
        var activePaths = new List<NextStep>() { firstStep };
        while (true)
        {
            // record these steps as seen
            activePaths.ForEach(s => seenSteps.Add(s));
            
            // determine next steps
            activePaths = activePaths
                .SelectMany(ns => EnergizeAndGetNext(grid, ns.Coordinate, ns.Direction, x => energizedTiles.Add(x)))
                .ToList();
            
            // remove any next steps we've already seen
            activePaths = activePaths
                .Where(ns => !seenSteps.Contains(ns))
                .ToList();
            
            if(activePaths.Count == 0) break;
        }
        
        // Debug($"Energized Grid:");
        // Debug(Visualize(grid, energizedTiles.ToArray()).Indent());
        
        var totalEnergizedTiles = energizedTiles.Count;
        return totalEnergizedTiles;
    }
    
    private string Visualize(char[,] grid, GridCoordinate[] energized)
    {
        var sb = new StringBuilder();
        for (var row = 0; row < grid.GetLength(0); row++)
        {
            for (var column = 0; column < grid.GetLength(1); column++)
            {
                var coordinate = new GridCoordinate(row, column);
                if (energized.Contains(coordinate))
                {
                    sb.Append('#');
                }
                else
                {
                    sb.Append(grid[row, column]);
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public record GridCoordinate(int Row, int Column)
    {
        public GridCoordinate Next(Direction direction)
        {
            return direction switch
            {
                Direction.Up => new GridCoordinate(Row - 1, Column),
                Direction.Down => new GridCoordinate(Row + 1, Column),
                Direction.Left => new GridCoordinate(Row, Column - 1),
                Direction.Right => new GridCoordinate(Row, Column + 1),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        public bool IsInBounds(char[,] grid)
        {
            return Row >= 0 && Row < grid.GetLength(0) && Column >= 0 && Column < grid.GetLength(1);
        }
    }
    
    public enum Direction { Up, Down, Left, Right }
    
    public record NextStep(GridCoordinate Coordinate, Direction Direction);

    public NextStep[] EnergizeAndGetNext(char[,] grid, GridCoordinate start, Direction direction, Action<GridCoordinate> energizeTile)
    {
        // mark this tile as energized
        var current = start;
        energizeTile(current);
        // figure out where we go next
        var nextDirections = (direction, grid[current.Row, current.Column]) switch
        {
            (Direction.Up, '.') => new[]{Direction.Up},
            (Direction.Up, '|') => new[]{Direction.Up},
            (Direction.Up, '\\') => new[]{Direction.Left},
            (Direction.Up, '/') => new[]{Direction.Right},
            (Direction.Up, '-') => new[]{Direction.Left, Direction.Right},
            
            (Direction.Right, '.') => new[]{Direction.Right},
            (Direction.Right, '|') => new[]{Direction.Up, Direction.Down},
            (Direction.Right, '\\') => new[]{Direction.Down},
            (Direction.Right, '/') => new[]{Direction.Up},
            (Direction.Right, '-') => new[]{Direction.Right},
            
            (Direction.Down, '.') => new[]{Direction.Down},
            (Direction.Down, '|') => new[]{Direction.Down},
            (Direction.Down, '\\') => new[]{Direction.Right},
            (Direction.Down, '/') => new[]{Direction.Left},
            (Direction.Down, '-') => new[]{Direction.Left, Direction.Right},
            
            (Direction.Left, '.') => new[]{Direction.Left},
            (Direction.Left, '|') => new[]{Direction.Up, Direction.Down},
            (Direction.Left, '\\') => new[]{Direction.Up},
            (Direction.Left, '/') => new[]{Direction.Down},
            (Direction.Left, '-') => new[]{Direction.Left},
            
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var validNextSteps = nextDirections
            .Select(d => new NextStep(current.Next(d), d))
            .Where(ns => ns.Coordinate.IsInBounds(grid))
            .ToArray();

        return validNextSteps;
    }

}

