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

    [Fact] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(2);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var histories = ParseHistories(data);
        WriteLine($"Part 1 - Loaded {histories.Count} Histories");
        
        var nextValues = histories.Select(Extrapolate).Select(x => x.NextValue).ToList();

        var totalOfNextValues = nextValues.Sum();
        WriteLine($"Total of Next Values: {totalOfNextValues}");
        return totalOfNextValues;
    }
    
    public long ExecutePart2(string data)
    {
        var histories = ParseHistories(data);
        WriteLine($"Part 2 - Loaded {histories.Count} Histories");

        var previousValues = histories.Select(Extrapolate).Select(x => x.PreviousValue).ToList();

        var totalOfPreviousValues = previousValues.Sum();
        WriteLine($"Total of Previous Values: {totalOfPreviousValues}");
        return totalOfPreviousValues;
    }

    public List<History> ParseHistories(string data)
    {
        var histories = data.ToLines(true)
            .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList())
            .Select(values => new History(values))
            .ToList();
        return histories;
    }

    public HistoryExtrapolation Extrapolate(History history)
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

        // step back up from the bottom and add on "previous" and "next" values to each line
        long lowerSequenceStart = 0;
        long lowerSequenceEnd = 0;
        for (int i = (sequences.Count-1); i >= 0; i--)
        {
            var currentSeq = sequences[i];
            
            currentSeq.Insert(0, currentSeq.First() - lowerSequenceStart);
            lowerSequenceStart = currentSeq.First();
            
            currentSeq.Add(currentSeq.Last() + lowerSequenceEnd);
            lowerSequenceEnd = currentSeq.Last();
        }
        
        sequences.ForEach(seq => Debug(seq.Select(l => l.ToString()).JoinWith(" ")));
        Debug($"");

        var nextValue = sequences.First().Last();
        var previousValue = sequences.First().First();
        return new HistoryExtrapolation(history, previousValue, nextValue);
    }
    
    public class History
    {
        public History(List<long> values) => Values = values;
        public List<long> Values { get; }
    }
    
    public record HistoryExtrapolation(History History, long PreviousValue, long NextValue);

}

