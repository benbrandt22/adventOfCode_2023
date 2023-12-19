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

    [Fact][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(952408144115);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var steps = ParseDataPart1(data);
        WriteLine($"Part 1 - Loaded Dig Plan with {steps.Count} steps");
        var area = GetAreaOfDigPlan(steps);
        WriteLine($"Total cubic meters dug: {area}");
        return (long)area;
    }
    
    public long ExecutePart2(string data)
    {
        var steps = ParseDataPart2(data);
        WriteLine($"Part 2 - Loaded Dig Plan with {steps.Count} steps");
        var area = GetAreaOfDigPlan(steps);
        WriteLine($"Total cubic meters dug: {area}");
        return (long)area;
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
    
    public record DigStep(Direction Direction, long Distance);
    public record Direction(long Dx, long Dy);

    public record Coordinate(long X, long Y)
    {
        public Coordinate Move(Direction direction, long distance = 1) =>
            new(X + (direction.Dx * distance), Y + (direction.Dy * distance));
    }
    
    public static readonly Direction Up = new(0, 1);
    public static readonly Direction Down = new(0, -1);
    public static readonly Direction Left = new(-1, 0);
    public static readonly Direction Right = new(1, 0);

    public List<DigStep> ParseDataPart1(string data)
    {
        var lineRegEx = new Regex(@"(?<direction>[UDLR])\W(?<distance>\d+)\W\(\#(?<hex>[a-f0-9]+)\)");

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

                return new DigStep(direction, distance);
            })
            .ToList();

        return steps;
    }
    
    public List<DigStep> ParseDataPart2(string data)
    {
        var lineRegEx = new Regex(@"(?<direction>[UDLR])\W(?<distance>\d+)\W\(\#(?<hex>[a-f0-9]{6})\)");

        var steps = data.ToLines(true)
            .Select(line => lineRegEx.Match(line))
            .Select(m =>
            {
                // The last hexadecimal digit encodes the direction to dig: 0 means R, 1 means D, 2 means L, and 3 means U
                var hexValue = m.Groups["hex"].Value;
                
                var direction = hexValue.Last() switch
                {
                    '3' => Up, '1' => Down, '2' => Left, '0' => Right,
                    _ => throw new Exception("Invalid direction")
                };

                var distance = int.Parse(hexValue.Substring(0,5), System.Globalization.NumberStyles.HexNumber);

                return new DigStep(direction, distance);
            })
            .ToList();

        return steps;
    }

    /// <summary>
    /// Calculates the area of a polygon from coordinates of each vertex using the shoelace formula
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/Shoelace_formula
    /// </remarks>
    public double PolygonAreaShoelaceFormula(List<Coordinate> coordinates)
    {
        var n = coordinates.Count;
        double determinants = 0.0;
        for (int i = 0; i < n; i++)
        {
            var j = (i + 1) % n;
            determinants += coordinates[i].X * coordinates[j].Y;
            determinants -= coordinates[i].Y * coordinates[j].X;
        }
        var area = Math.Abs(determinants / 2);
        return area;
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
            var j = (i + 1) % coordinates.Count; // get next coordinate index or wrap around to first coordinate
            var lineSegmentLength = Math.Sqrt(Math.Pow(coordinates[j].X - coordinates[i].X, 2) +
                                              Math.Pow(coordinates[j].Y - coordinates[i].Y, 2));
            perimeter += lineSegmentLength;
        }
        return perimeter;
    }
    
}