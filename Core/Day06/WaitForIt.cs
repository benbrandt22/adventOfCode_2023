using System.Diagnostics;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day06;

public class WaitForIt : BaseDayModule
{
    public WaitForIt(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 6;
    public override string Title => "Wait For It";

    [Fact] public void Part1_Sample() => Execute(LoadRacesPart1(GetData(InputType.Sample))).Should().Be(288);
    [Fact] public void Part1() => Execute(LoadRacesPart1(GetData(InputType.Input)));

    [Fact] public void Part2_Sample() => Execute(LoadRacesPart2(GetData(InputType.Sample))).Should().Be(71503);
    [Fact] public void Part2() => Execute(LoadRacesPart2(GetData(InputType.Input)));

    public List<BoatRace> LoadRacesPart1(string data) => ParseRaces(data, false);
    public List<BoatRace> LoadRacesPart2(string data) => ParseRaces(data, true);

    public long Execute(List<BoatRace> races)
    {
        WriteLine($"Loaded {races.Count} {(races.Count == 1 ? "race" : "races")}:");
        races.ForEach(r => WriteLine($" {r.RaceDurationMs} ms, {r.RecordDistanceMm} mm"));
        
        var productOfWaysToWin = races
            .Select(r => r.FindTotalWaysToWin())
            .Aggregate(1L, (acc, next) => acc * next);
        
        WriteLine($"Total ways to win for each race, multiplied: {productOfWaysToWin}");
        return productOfWaysToWin;
    }

    public List<BoatRace> ParseRaces(string inputText, bool condenseSpaces)
    {
        var results = new List<BoatRace>();
        var lines = inputText.ToLines();
        var times = lines[0].Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Skip(1).Select(long.Parse).ToList();
        var distances = lines[1].Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Skip(1).Select(long.Parse).ToList();

        if (condenseSpaces)
        {
            // (for part 2, condense the numbers into a single big number)
            times = new List<long> { long.Parse(times.Select(x => x.ToString()).JoinWith("")) };
            distances = new List<long> { long.Parse(distances.Select(x => x.ToString()).JoinWith("")) };
        }

        for (int i = 0; i < times.Count; i++)
        {
            results.Add(new BoatRace(times[i], distances[i]));
        }

        return results;
    }

    [DebuggerDisplay("{RaceDurationMs} ms, {RecordDistanceMm} mm")]
    public class BoatRace
    {
        public BoatRace(long raceDurationMs, long recordDistanceMm)
        {
            RaceDurationMs = raceDurationMs;
            RecordDistanceMm = recordDistanceMm;
        }
        public long RaceDurationMs { get; set; }
        public long RecordDistanceMm { get; set; }

        public long FindTotalWaysToWin()
        {
            long winningOutcomes = 0;
            for (int buttonTime = 1; buttonTime < RaceDurationMs; buttonTime++)
            {
                var travelTimeMs = RaceDurationMs - buttonTime;
                var speedMmPerMs = buttonTime;
                var distanceTraveledMm = speedMmPerMs * travelTimeMs;
                // does the distance traveled beat the record distance?
                if (distanceTraveledMm > RecordDistanceMm)
                {
                    winningOutcomes++;
                }
            }
            return winningOutcomes;
        }
    }
}

