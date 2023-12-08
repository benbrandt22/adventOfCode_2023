using System.Collections;
using System.Text.RegularExpressions;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day05;

public class SeedFertilizer : BaseDayModule
{
    public SeedFertilizer(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 5;
    public override string Title => "If You Give A Seed A Fertilizer";

    [Fact] public void Part1_Sample() => Part1_FindClosestDestination(LoadAlmanac(GetData(InputType.Sample))).Should().Be(35);
    [Fact] public void Part1() => Part1_FindClosestDestination(LoadAlmanac(GetData(InputType.Input)));

    [Fact] public void Part2_Sample() => Part2_FindClosestDestinationUsingSeedRanges(LoadAlmanac(GetData(InputType.Sample))).Should().Be(46);
    
    [Fact(Skip = "Too slow to be included in test run, should refactor using ranges for a *proper* solution")]
    public void Part2() => Part2_FindClosestDestinationUsingSeedRanges(LoadAlmanac(GetData(InputType.Input)));
    
    private Almanac LoadAlmanac(string data)
    {
        return ParseInput(data);
    }
    
    public long Part1_FindClosestDestination(Almanac almanac)
    {
        var seedValues = almanac.SeedInputValues;
        var closestDestination = FindClosestDestination(almanac, seedValues);
        return closestDestination;
    }
    
    public long Part2_FindClosestDestinationUsingSeedRanges(Almanac almanac)
    {
        var numberOfInputRanges = almanac.SeedInputValues.Count / 2;
        var ranges = Enumerable.Range(0, numberOfInputRanges)
            .Select(rangeNum =>
            {
                var longRange = new LongRange(almanac.SeedInputValues[rangeNum * 2],
                    almanac.SeedInputValues[(rangeNum * 2) + 1]);
                WriteLine($" Seed Range {rangeNum}: Start {longRange.Start} Count {longRange.Count}");
                return longRange;
            }).ToList();

        var seedValues = ranges[0].Values();
        ranges.Skip(1).ToList().ForEach(r => seedValues = seedValues.Concat(r.Values()));
        
        var closestDestination = FindClosestDestination(almanac, seedValues);
        return closestDestination;
    }
    
    public long FindClosestDestination(Almanac almanac, IEnumerable<long> seedValues)
    {
        var results = seedValues.Select(s =>
        {
            var stageInput = new MapStage(MapCategory.Seed, s);
            var stageOutput = almanac.Map(stageInput);
            //WriteLine($" {stageInput.Category} {stageInput.Value} -> {stageOutput.Category} {stageOutput.Value}");
            return new MapStageResult(stageInput, stageOutput);
        });

        var minimumDestination = results.MinBy(r => r.Destination.Value)!;
        WriteLine();
        WriteLine($"Minimum destination: {minimumDestination.Destination.Category} {minimumDestination.Destination.Value}");
        return minimumDestination.Destination.Value;
    }
    
    private Almanac ParseInput(string input)
    {
        var seeds = new Regex(@"\d+").Matches(input.ToLines().First()).Select(m => long.Parse(m.Value)).ToList();
        var maps = input.ToParagraphs().Skip(1).Select(ParseMapSection).ToList();

        var almanac = new Almanac()
        {
            SeedInputValues = seeds,
            Maps = maps
        };

        return almanac;
    }

    private AlmanacMap ParseMapSection(string text)
    {
        var categoriesRegEx = new Regex(@"(?<Source>[a-z]+)-to-(?<Destination>[a-z]+) map", RegexOptions.None);
        var map = categoriesRegEx.Match(text).MapTo<AlmanacMap>();
        
        var rangesRegEx = new Regex(@"^(?<destinationStart>\d+) (?<sourceStart>\d+) (?<rangeLength>\d+)$", RegexOptions.None);

        map.Ranges = text.ToLines(true).Skip(1).Select(line => rangesRegEx.Match(line).MapTo<AlmanacMapRange>()).ToList();
        
        return map;
    }

    public class Almanac()
    {
        public List<long> SeedInputValues { get; set; }
        public List<AlmanacMap> Maps { get; set; }
        
        public MapStage Map(MapStage stage)
        {
            while (true)
            {
                var map = Maps.SingleOrDefault(map => map.CanMap(stage));
                if (map == null) return stage;
                stage = map.Map(stage);
            }
        }
    }

    public class AlmanacMap()
    {
        public MapCategory Source { get; set; }
        public MapCategory Destination { get; set; }
        public List<AlmanacMapRange> Ranges { get; set; }

        public bool CanMap(MapStage inputStage)
        {
            return inputStage.Category == Source;
        }
        
        public MapStage Map(MapStage inputStage)
        {
            if(!CanMap(inputStage)) throw new Exception($"Cannot map {inputStage.Category} to {Destination}");
            var mappingRange = Ranges.SingleOrDefault(range => range.CanMap(inputStage.Value));
            var newValue = mappingRange?.Map(inputStage.Value) ?? inputStage.Value;
            return new MapStage(Destination, newValue);
        }
    }

    public class AlmanacMapRange(long destinationStart, long sourceStart, long rangeLength)
    {
        public long DestinationMin { get; } = destinationStart;
        public long DestinationMax { get; } = destinationStart + (rangeLength - 1);
        public long SourceMin { get; } = sourceStart;
        public long SourceMax { get; } = sourceStart + (rangeLength - 1);
        
        public bool CanMap(long input)
        {
            return input >= SourceMin && input <= SourceMax;
        }

        public long Map(long input)
        {
            if(!CanMap(input)) throw new Exception($"Cannot map {input} to {DestinationMin}-{DestinationMax}");
            return input - SourceMin + DestinationMin;
        }
    }

    public enum MapCategory {
        Seed, Soil, Fertilizer, Water, Light, Temperature, Humidity, Location
    }

    public record MapStage(MapCategory Category, long Value);

    private record MapStageResult(MapStage Source, MapStage Destination);

    public class LongRange(long start, long count)
    {
        public long Start { get; } = start;
        public long Count { get; } = count;
        public long End { get; } = start + count - 1;

        public IEnumerable<long> Values()
        {
            var end = End;
            for (long i = Start; i < end; i++)
            {
                yield return i;
            }
        }
    }
}