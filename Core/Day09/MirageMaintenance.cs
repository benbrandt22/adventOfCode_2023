using System.Diagnostics;
using System.Runtime.CompilerServices;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day09;

public class MirageMaintenance : BaseDayModule
{
    public MirageMaintenance(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 9;
    public override string Title => "Mirage Maintenance";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(114);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not yet implemented")] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var histories = ParseHistories(data);
        WriteLine($"Part 1 - Loaded {histories.Count} Histories");
        
        var nextValues = histories.Select(FindNextValue).ToList();

        var totalOfNextValues = nextValues.Sum();
        WriteLine($"Total of Next Values: {totalOfNextValues}");
        return totalOfNextValues;
    }
    
    public int ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }

    public List<History> ParseHistories(string data)
    {
        var histories = data.ToLines(true)
            .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList())
            .Select(values => new History(values))
            .ToList();
        return histories;
    }

    public long FindNextValue(History history)
    {
        var sequences = new List<List<long>> { history.Values };
        while (sequences.Last().Any(v => v != 0))
        {
            var diffs = new List<long>();
            var previousValues = sequences.Last();
            for (int i = 1; i < previousValues.Count; i++)
            {
                diffs.Add(previousValues[i] - previousValues[i - 1]);
            }
            sequences.Add(diffs);
        }

        // step back up from the bottom and add on "next" values to each line
        long lowerSequenceEnd = 0;
        for (int i = (sequences.Count-1); i >= 0; i--)
        {
            var currentSeq = sequences[i];
            currentSeq.Add(currentSeq.Last() + lowerSequenceEnd);
            lowerSequenceEnd = currentSeq.Last();
        }
        
        sequences.ForEach(seq => Debug(seq.Select(l => l.ToString()).JoinWith(" ")));
        Debug($"");

        var nextValue = sequences.First().Last();
        return nextValue;
    }
    
    public class History
    {
        public History(List<long> values) => Values = values;
        public List<long> Values { get; }
    }

}

