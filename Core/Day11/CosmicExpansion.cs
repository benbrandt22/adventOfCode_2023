using System.Diagnostics;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day11;

public class CosmicExpansion : BaseDayModule
{
    public CosmicExpansion(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 11;
    public override string Title => "Cosmic Expansion";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(374);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not yet implemented")] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public int ExecutePart1(string data)
    {
        var lines = data.ToLines(true).ToList();
        WriteLine($"Part 1 - Loaded telescope image - Width: {lines[0].Length}, Height: {lines.Count}");
        var skyMapLines = ExpandTelescopeImage(lines);
        WriteLine($"Expanded telescope image - Width: {skyMapLines[0].Length}, Height: {skyMapLines.Count}");
        
        // locate all galaxies
        var galaxyCoordinates = new List<GalaxyCoordinate>();
        for (int rowIndex = 0; rowIndex < skyMapLines.Count; rowIndex++)
        {
            var line = skyMapLines[rowIndex];
            var galaxyCoordinatesOnThisRow = line.AllIndexesOf("#", StringComparison.OrdinalIgnoreCase)
                .Select(i => new GalaxyCoordinate(i, rowIndex)).ToList();
            galaxyCoordinates.AddRange(galaxyCoordinatesOnThisRow);
        }
        var galaxies = galaxyCoordinates.Select((gc, i) => new Galaxy(i, gc)).ToList();
        WriteLine($"Located {galaxies.Count} galaxies");
        
        // find all pairs based on: https://stackoverflow.com/a/7242116
        var galaxyPairs = galaxies
            .SelectMany(x => galaxies, (x, y) => new GalaxyPair(x, y))
            .Where(gp => gp.GalaxyA.Id < gp.GalaxyB.Id)
            .ToList();
        
        WriteLine($"Identified {galaxyPairs.Count} galaxy pairs");
        
        var totalPairDistances = galaxyPairs.Sum(gp => gp.FindGridPathDistance());
        
        WriteLine($"Total Galaxy-Pair Distances: {totalPairDistances}");
        return totalPairDistances;
    }

    private List<string> ExpandTelescopeImage(List<string> lines)
    {
        var columnIndexesOfEmptiness = Enumerable.Range(0, lines[0].Length)
            .Where(i => lines.All(line => line[i] == '.'))
            .OrderDescending()
            .ToList();
        var rowIndexesOfEmptiness = Enumerable.Range(0, lines.Count)
            .Where(i => lines[i].All(c => c == '.'))
            .OrderDescending()
            .ToList();

        // insert new columns
        lines = lines
            .Select(line =>
            {
                foreach (var ci in columnIndexesOfEmptiness)
                {
                    line = line.Insert(ci, ".");
                }

                return line;
            })
            .ToList();
        
        // insert new rows
        rowIndexesOfEmptiness.ForEach(ri => lines.Insert(ri, new string('.', lines[0].Length)));
        
        return lines;
    }

    public int ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }
    
    public record GalaxyCoordinate(int X, int Y);

    public record Galaxy(int Id, GalaxyCoordinate Location);

    public record GalaxyPair(Galaxy GalaxyA, Galaxy GalaxyB)
    {
        public int FindGridPathDistance()
        {
            var xDistance = Math.Abs(GalaxyA.Location.X - GalaxyB.Location.X);
            var yDistance = Math.Abs(GalaxyA.Location.Y - GalaxyB.Location.Y);
            return xDistance + yDistance;
        }
    }

}

