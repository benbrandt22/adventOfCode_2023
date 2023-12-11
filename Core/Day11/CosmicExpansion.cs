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

    [Fact] public void Part1_Sample() => AnalyzeGalaxies(GetData(InputType.Sample), 2).Should().Be(374);
    [Fact] public void Part1() => AnalyzeGalaxies(GetData(InputType.Input), 2);

    [Fact] public void Part2_Sample() => AnalyzeGalaxies(GetData(InputType.Sample), 10).Should().Be(1030);
    [Fact] public void Part2_Sample2() => AnalyzeGalaxies(GetData(InputType.Sample), 100).Should().Be(8410);
    [Fact] public void Part2() => AnalyzeGalaxies(GetData(InputType.Input), 1000000);

    public long AnalyzeGalaxies(string data, int expansionFactor)
    {
        var skyMapLines = data.ToLines(true).ToList();
        WriteLine($"Loaded telescope image - Width: {skyMapLines[0].Length}, Height: {skyMapLines.Count}");
        var expansionZones = FindExpansionZones(skyMapLines);
        WriteLine($"Expansion Zones: {expansionZones.RowIndexes.Count} rows, {expansionZones.ColumnIndexes.Count} columns");

        var galaxies = LocateGalaxies(skyMapLines);
        WriteLine($"Located {galaxies.Count} galaxies");
        
        var galaxyPairs = IdentifyGalaxyPairs(galaxies);
        WriteLine($"Identified {galaxyPairs.Count} galaxy pairs");
        
        var totalPairDistances = galaxyPairs.Sum(gp => gp.FindGridPathDistance(expansionZones, expansionFactor));
        
        WriteLine($"Total Galaxy-Pair Distances (Expansion factor {expansionFactor}): {totalPairDistances}");
        return totalPairDistances;
    }

    public List<Galaxy> LocateGalaxies(List<string> skyMapLines)
    {
        var galaxyCoordinates = new List<GalaxyCoordinate>();
        for (int rowIndex = 0; rowIndex < skyMapLines.Count; rowIndex++)
        {
            var line = skyMapLines[rowIndex];
            var galaxyCoordinatesOnThisRow = line.AllIndexesOf("#", StringComparison.OrdinalIgnoreCase)
                .Select(i => new GalaxyCoordinate(i, rowIndex)).ToList();
            galaxyCoordinates.AddRange(galaxyCoordinatesOnThisRow);
        }
        var galaxies = galaxyCoordinates.Select((gc, i) => new Galaxy(i, gc)).ToList();
        return galaxies;
    }

    public List<GalaxyPair> IdentifyGalaxyPairs(List<Galaxy> galaxies)
    {
        // find all pairs based on: https://stackoverflow.com/a/7242116
        var galaxyPairs = galaxies
            .SelectMany(x => galaxies, (x, y) => new GalaxyPair(x, y))
            .Where(gp => gp.GalaxyA.Id < gp.GalaxyB.Id)
            .ToList();
        return galaxyPairs;
    }

    private ExpansionZones FindExpansionZones(List<string> skyMapLines)
    {
        var columnIndexesOfEmptiness = Enumerable.Range(0, skyMapLines[0].Length)
            .Where(i => skyMapLines.All(line => line[i] == '.'))
            .ToList();
        var rowIndexesOfEmptiness = Enumerable.Range(0, skyMapLines.Count)
            .Where(i => skyMapLines[i].All(c => c == '.'))
            .ToList();

        return new ExpansionZones(columnIndexesOfEmptiness, rowIndexesOfEmptiness);
    }

    public record ExpansionZones(List<int> ColumnIndexes, List<int> RowIndexes);
    
    public record GalaxyCoordinate(long X, long Y);

    public record Galaxy(int Id, GalaxyCoordinate Location);

    public record GalaxyPair(Galaxy GalaxyA, Galaxy GalaxyB)
    {
        public long FindGridPathDistance(ExpansionZones expansionZones, int expansionFactor)
        {
            // uses "Taxicab Geometry" or "Manhattan Distance" to find the distance between two points
            // https://en.wikipedia.org/wiki/Taxicab_geometry
            var xDistance = Math.Abs(GalaxyA.Location.X - GalaxyB.Location.X);
            
            var minX = Math.Min(GalaxyA.Location.X, GalaxyB.Location.X);
            var maxX = Math.Max(GalaxyA.Location.X, GalaxyB.Location.X);
            var expansionColumnsCrossed = expansionZones.ColumnIndexes.Count(ci => ci >= minX && ci <= maxX);
            
            var yDistance = Math.Abs(GalaxyA.Location.Y - GalaxyB.Location.Y);
            
            var minY = Math.Min(GalaxyA.Location.Y, GalaxyB.Location.Y);
            var maxY = Math.Max(GalaxyA.Location.Y, GalaxyB.Location.Y);
            var expansionRowsCrossed = expansionZones.RowIndexes.Count(ri => ri >= minY && ri <= maxY);
            
            return xDistance
                   + (expansionColumnsCrossed * (expansionFactor - 1))
                   + yDistance
                   + (expansionRowsCrossed * (expansionFactor - 1));
        }
    }

}