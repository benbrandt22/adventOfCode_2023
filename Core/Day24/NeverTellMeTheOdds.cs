using System.Numerics;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day24;

public class NeverTellMeTheOdds : BaseDayModule
{
    public NeverTellMeTheOdds(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 24;
    public override string Title => "Never Tell Me The Odds";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample), 7, 27).Should().Be(2);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input), 200000000000000, 400000000000000);

    [Fact(Skip = "Not yet implemented")][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data, long xyMin, long xyMax)
    {
        var hailstones = ParseHailstones(data);
        WriteLine($"Part 1 - Loaded {hailstones.Count} Hailstones");
        var hailstonePairs = IdentifyHailstonePairs(hailstones);
        WriteLine($"Identified {hailstonePairs.Count} Hailstone Pairs to analyze");

        var totalCrossingPairs = hailstonePairs
            .Count(p => WillCrossPathsInTargetArea(p, xyMin, xyMax, xyMin, xyMax));

        WriteLine($"Total Crossing Pairs: {totalCrossingPairs}");
        return totalCrossingPairs;
    }
    
    public long ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }

    public record Hailstone(int Id, double X, double Y, double Z, double Vx, double Vy, double Vz);
    
    public List<Hailstone> ParseHailstones(string data) =>
        data.ToLines(true).Select((line, index) =>
        {
            var parts = line.Split(',', '@');
            var x = double.Parse(parts[0]);
            var y = double.Parse(parts[1]);
            var z = double.Parse(parts[2]);
            var vx = double.Parse(parts[3]);
            var vy = double.Parse(parts[4]);
            var vz = double.Parse(parts[5]);
            return new Hailstone(index, x, y, z, vx, vy, vz);
        }).ToList();
    
    public List<HailstonePair> IdentifyHailstonePairs(List<Hailstone> hailstones)
    {
        // find all pairs based on: https://stackoverflow.com/a/7242116
        var hailstonePairs = hailstones
            .SelectMany(x => hailstones, (x, y) => new HailstonePair(x, y))
            .Where(hp => hp.HailstoneA.Id < hp.HailstoneB.Id)
            .ToList();
        return hailstonePairs;
    }

    public record HailstonePair(Hailstone HailstoneA, Hailstone HailstoneB);

    /// <summary>
    /// Calculate the time of intersection between two Hailstones in the XY plane, ignoring the Z axis.
    /// </summary>
    public bool WillCrossPathsInTargetArea(HailstonePair hailstonePair, long xMin, long xMax, long yMin, long yMax)
    {
        var h1 = hailstonePair.HailstoneA;
        var h2 = hailstonePair.HailstoneB;

        // adapted this from other solutions, so I can't say I totally follow the math right now, but it
        // looks like we're generating two lines from the starting points (t=0) and some generated points at t=1,
        // then finding the intersection of those lines
        var x1 = h1.X;
        var x2 = h1.X + h1.Vx;
        var x3 = h2.X;
        var x4 = h2.X + h2.Vx;
        var y1 = h1.Y;
        var y2 = h1.Y + h1.Vy;
        var y3 = h2.Y;
        var y4 = h2.Y + h2.Vy;

        var denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
        if (denom == 0) return false;
        var ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
        var ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

        // point of intersection
        var x = x1 + ua * (x2 - x1);
        var y = y1 + ua * (y2 - y1);

        // check if event happens in the future (see if the intersection point is on the "time-positive" side of the starting point)
        if (x > x1 == x2 - x1 > 0 && y > y1 == y2 - y1 > 0 && x > x3 == x4 - x3 > 0 && y > y3 == y4 - y3 > 0)
        {
            // now check if intersection point is within the target area
            if (x >= xMin && x <= xMax && y >= yMin && y <= yMax)
            {
                return true;
            }
        }

        return false;
    }


}

