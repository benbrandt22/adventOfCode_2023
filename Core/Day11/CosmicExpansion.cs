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

    [Fact] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample), 10).Should().Be(1030);
    [Fact] public void Part2_Sample2() => ExecutePart2(GetData(InputType.Sample), 100).Should().Be(8410);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input), 1000000);

    public long ExecutePart1(string data)
    {
        var lines = data.ToLines(true).ToList();
        WriteLine($"Part 1 - Loaded telescope image - Width: {lines[0].Length}, Height: {lines.Count}");
        var skyMapLines = ExpandTelescopeImage(lines);
        WriteLine($"Expanded telescope image - Width: {skyMapLines[0].Length}, Height: {skyMapLines.Count}");
        
        var galaxies = LocateGalaxies(skyMapLines);
        WriteLine($"Located {galaxies.Count} galaxies");
        
        var galaxyPairs = IdentifyGalaxyPairs(galaxies);
        WriteLine($"Identified {galaxyPairs.Count} galaxy pairs");
        
        var totalPairDistances = galaxyPairs.Sum(gp => gp.FindGridPathDistance());
        
        WriteLine($"Total Galaxy-Pair Distances: {totalPairDistances}");
        return totalPairDistances;
    }
    
    public long ExecutePart2(string data, int expansionFactor)
    {
        var skyMapLines = data.ToLines(true).ToList();
        WriteLine($"Part 2 - Loaded telescope image - Width: {skyMapLines[0].Length}, Height: {skyMapLines.Count}");
        var expansionZones = FindExpansionZones(skyMapLines);
        WriteLine($"Expansion Zones: {expansionZones.RowIndexes.Count} rows, {expansionZones.ColumnIndexes.Count} columns");

        var galaxies = LocateGalaxies(skyMapLines);
        WriteLine($"Located {galaxies.Count} galaxies");
        
        var galaxyPairs = IdentifyGalaxyPairs(galaxies);
        WriteLine($"Identified {galaxyPairs.Count} galaxy pairs");
        
        var totalPairDistances = galaxyPairs.Sum(gp => gp.FindGridPathDistance(expansionZones, expansionFactor));
        
        WriteLine($"Total Galaxy-Pair Distances: {totalPairDistances}");
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

    private List<string> ExpandTelescopeImage(List<string> lines)
    {
        var expansionZones = FindExpansionZones(lines);

        // insert new columns
        lines = lines
            .Select(line =>
            {
                foreach (var ci in expansionZones.ColumnIndexes.OrderDescending())
                {
                    line = line.Insert(ci, ".");
                }

                return line;
            })
            .ToList();
        
        // insert new rows
        expansionZones.RowIndexes.OrderDescending().ToList()
            .ForEach(ri => lines.Insert(ri, new string('.', lines[0].Length)));
        
        return lines;
    }
    
    private ExpansionZones FindExpansionZones(List<string> skyMapLines)
    {
        var columnIndexesOfEmptiness = Enumerable.Range(0, skyMapLines[0].Length)
            .Where(i => skyMapLines.All(line => line[i] == '.'))
            .OrderDescending()
            .ToList();
        var rowIndexesOfEmptiness = Enumerable.Range(0, skyMapLines.Count)
            .Where(i => skyMapLines[i].All(c => c == '.'))
            .OrderDescending()
            .ToList();

        return new ExpansionZones(columnIndexesOfEmptiness, rowIndexesOfEmptiness);
    }

    public record ExpansionZones(List<int> ColumnIndexes, List<int> RowIndexes);
    
    public record GalaxyCoordinate(long X, long Y);

    public record Galaxy(int Id, GalaxyCoordinate Location);

    public record GalaxyPair(Galaxy GalaxyA, Galaxy GalaxyB)
    {
        public long FindGridPathDistance()
        {
            var xDistance = Math.Abs(GalaxyA.Location.X - GalaxyB.Location.X);
            var yDistance = Math.Abs(GalaxyA.Location.Y - GalaxyB.Location.Y);
            return xDistance + yDistance;
        }
        
        public long FindGridPathDistance(ExpansionZones expansionZones, int expansionFactor)
        {
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

