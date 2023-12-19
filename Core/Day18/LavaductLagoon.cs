using System.Drawing;
using System.Text.RegularExpressions;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day18;

public class LavaductLagoon : BaseDayModule
{
    public LavaductLagoon(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 18;
    public override string Title => "Lavaduct Lagoon";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(62);
    [Fact][ShowDebug] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not yet implemented")][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var steps = ParseData(data);
        WriteLine($"Part 1 - Loaded Dig Plan with {steps.Count} steps");

        var area = GetAreaOfDigPlan(steps);
        
        WriteLine($"Total cubic meters dug: {area}");
        return (long)area;
    }
    
    public long ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }

    private double GetAreaOfDigPlan(List<DigStep> steps)
    {
        var location = new Coordinate(0, 0);
        var borderCoordinates = new List<Coordinate>();
        foreach (var step in steps)
        {
            borderCoordinates.Add(location);
            location = location.Move(step.Direction, step.Distance);
        }

        var totalArea = PolygonAreaShoelaceFormula_OneUnitBorder(borderCoordinates);
        
        return totalArea;
    }
    
    public record DigStep(Direction Direction, int Distance, Color Color);
    public record Direction(int Dx, int Dy);

    public record Coordinate(int X, int Y)
    {
        public Coordinate Move(Direction direction, int distance = 1) =>
            new(X + (direction.Dx * distance), Y + (direction.Dy * distance));
    }
    
    public static readonly Direction Up = new(0, 1);
    public static readonly Direction Down = new(0, -1);
    public static readonly Direction Left = new(-1, 0);
    public static readonly Direction Right = new(1, 0);

    public List<DigStep> ParseData(string data)
    {
        var lineRegEx = new Regex(@"(?<direction>[UDLR])\W(?<distance>\d+)\W\((?<hexcolor>\#[a-f0-9]+)\)");

        var steps = data.ToLines(true)
            .Select(line => lineRegEx.Match(line))
            .Select(m =>
            {
                var direction = m.Groups["direction"].Value switch
                {
                    "U" => Up, "D" => Down, "L" => Left, "R" => Right,
                    _ => throw new Exception("Invalid direction")
                };

                var distance = int.Parse(m.Groups["distance"].Value);
                var color = ColorTranslator.FromHtml(m.Groups["hexcolor"].Value);

                return new DigStep(direction, distance, color);
            })
            .ToList();

        return steps;
    }

    public double PolygonAreaShoelaceFormula(List<Coordinate> coordinates)
    {
        var n = coordinates.Count;
        double area = 0.0;
        for (int i = 0; i < n; i++)
        {
            var j = (i + 1) % n;
            area += coordinates[i].X * coordinates[j].Y;
            area -= coordinates[i].Y * coordinates[j].X;
        }

        area /= 2;
        return Math.Abs(area);
    }
    
    /// <summary>
    /// Similar to shoelace formula, but includes area of a border made up of 1 unit wide lines
    /// </summary>
    public double PolygonAreaShoelaceFormula_OneUnitBorder(List<Coordinate> coordinates)
    {
        var area = PolygonAreaShoelaceFormula(coordinates);
        var borderAdjustment = (PolygonPerimeter(coordinates) / 2) + 1;
        
        return area + borderAdjustment;
    }

    public double PolygonPerimeter(List<Coordinate> coordinates)
    {
        var perimeter = 0.0;
        for (int i = 0; i < coordinates.Count; i++)
        {
            var j = (i + 1) % coordinates.Count;
            perimeter += Math.Sqrt(Math.Pow(coordinates[j].X - coordinates[i].X, 2) + Math.Pow(coordinates[j].Y - coordinates[i].Y, 2));
        }

        return perimeter;
    }
    

}