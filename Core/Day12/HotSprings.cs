using System.Text.RegularExpressions;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day12;

public class HotSprings : BaseDayModule
{
    public HotSprings(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 12;
    public override string Title => "Hot Springs";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(21);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not yet implemented")] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(525152);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    [Fact]
    public void Unfold_Test()
    {
        var input = new SpringRecordRow("???.###", new List<int> { 1, 1, 3 });
        var unfolded = Unfold(input, 5);
        unfolded.ConditionLine.Should().Be("???.###????.###????.###????.###????.###");
        unfolded.DamagedGroupCounts.Should()
            .BeEquivalentTo(new List<int> { 1, 1, 3, 1, 1, 3, 1, 1, 3, 1, 1, 3, 1, 1, 3 });
    }
    
    public long ExecutePart1(string data)
    {
        var springRecordRows = ParseSpringRecords(data);
        WriteLine($"Part 1 - Loaded {springRecordRows.Count} rows of Spring records");
        
        var totalPossibleArrangements = springRecordRows.Sum(FindPossibleArrangements);

        WriteLine($"Total Possible Arrangements: {totalPossibleArrangements}");
        return totalPossibleArrangements;
    }

    public long ExecutePart2(string data)
    {
        var springRecordRows = ParseSpringRecords(data);
        WriteLine($"Part 2 - Loaded {springRecordRows.Count} rows of Spring records. Unfolding and analyzing..."); 
        springRecordRows = springRecordRows.Select(x => Unfold(x, 5)).ToList();

        // I can tell just the sample data will generate ridiculous amounts of possible arrangements, so brute forcing this seems like a poor choice
        throw new NotImplementedException();
    }

    private List<SpringRecordRow> ParseSpringRecords(string data) =>
        data.ToLines(true)
            .Select(line =>
            {
                var lineParts = line.Split(" ");
                var conditionLine = lineParts[0];
                var damagedGroupCounts = lineParts[1].Split(",").Select(int.Parse).ToList();
                return new SpringRecordRow(conditionLine, damagedGroupCounts);
            })
            .ToList();

    public record SpringRecordRow(string ConditionLine, List<int> DamagedGroupCounts);

    public long FindPossibleArrangements(SpringRecordRow springRecordRow)
    {
        long possibleSolutions = 0;
        
        var unknownIndexes = springRecordRow.ConditionLine
            .AllIndexesOf("?", StringComparison.OrdinalIgnoreCase).ToList();

        var damageGroupRegEx = new Regex(@"#+", RegexOptions.Compiled);
        
        // since each position can be represented as a operational or damaged, we can use binary to represent all possible combinations
        var totalPossibleCombinations = (long)Math.Pow(2, unknownIndexes.Count);
        for (long i = 0; i < totalPossibleCombinations; i++)
        {
            var binaryString = Convert.ToString(i, 2).PadLeft(unknownIndexes.Count, '0');
            var substitutions = binaryString.Replace('0', '#').Replace('1', '.');
            // drop the substitutions into the condition line
            var possibleConditionLine = springRecordRow.ConditionLine;
            for (int j = 0; j < unknownIndexes.Count; j++)
            {
                possibleConditionLine = possibleConditionLine.ReplaceAt(unknownIndexes[j], substitutions[j]);
            }
            // find the damage groupings in the possible solution line
            var damageGroups = damageGroupRegEx.Matches(possibleConditionLine).Select(x => x.Value.Length).ToList();
            
            // check if damage group counts match the expected counts
            if (springRecordRow.DamagedGroupCounts.SequenceEqual(damageGroups))
            {
                possibleSolutions++;
            }
        }

        return possibleSolutions;
    }

    public SpringRecordRow Unfold(SpringRecordRow springRecordRow, int folds)
    {
        var unfoldedConditionLine = Enumerable.Range(0, folds)
            .Select(_ => springRecordRow.ConditionLine)
            .JoinWith("?");

        var unfoldedDamagedGroupCounts = Enumerable.Repeat(springRecordRow.DamagedGroupCounts, folds)
            .SelectMany(x => x)
            .ToList();
        
        return new SpringRecordRow(unfoldedConditionLine, unfoldedDamagedGroupCounts);
    }

}

