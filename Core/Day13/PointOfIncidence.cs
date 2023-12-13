using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day13;

public class PointOfIncidence : BaseDayModule
{
    public PointOfIncidence(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 13;
    public override string Title => "Point of Incidence";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(405);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(400);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var patterns = ParsePatterns(data);
        WriteLine($"Part 1 - Loaded {patterns.Count} patterns");

        var totalReflectionResult = patterns
            .Select(p => FindReflection(p))
            .Sum(rr => rr.ResultValue());

        WriteLine($"Total Reflection Result: {totalReflectionResult}");
        return totalReflectionResult;
    }

    public long ExecutePart2(string data)
    {
        var patterns = ParsePatterns(data);
        WriteLine($"Part 2 - Loaded {patterns.Count} patterns");

        var totalReflectionResult = patterns
            .Select(FindReflectionAfterCleaningSmudge)
            .Sum(rr => rr.ResultValue());

        WriteLine($"Total Reflection Result: {totalReflectionResult}");
        return totalReflectionResult;
    }

    private ReflectionResult? FindReflection(Pattern pattern, ReflectionResult? ignoreResult = null)
    {
        var results = new List<ReflectionResult>();
        
        var horizontalLineReflectionIndexes = FindLastIndexesBeforeReflection(pattern.Lines);
        results.AddRange(horizontalLineReflectionIndexes.Select(i => new ReflectionResult(i, ReflectionLineDirection.Horizontal)));
        
        var verticalLineReflectionIndexes = FindLastIndexesBeforeReflection(RotateGrid(pattern.Lines));
        results.AddRange(verticalLineReflectionIndexes.Select(i => new ReflectionResult(i, ReflectionLineDirection.Vertical)));

        if (ignoreResult != null)
        {
            results = results.Where(r => r != ignoreResult).ToList();
        }
        
        return results.SingleOrDefault();
    }
    
    private List<string> RotateGrid(List<string> lines)
    {
        var rotatedLines = new List<string>();
        for (int i = 0; i < lines[0].Length; i++)
        {
            var newLine = new string(lines.Select(l => l[i]).ToArray());
            rotatedLines.Add(newLine);
        }

        return rotatedLines;
    }
    
    private List<int> FindLastIndexesBeforeReflection(List<string> lines)
    {
        List<Tuple<int,int>> adjacentPairs = new();
        for (int i = 0; i < lines.Count-1; i++)
        {
            if (lines[i] == lines[i + 1])
            {
                adjacentPairs.Add(new(i, i + 1));
            }
        }

        var reflectionLines = adjacentPairs
            .Where(p => IsReflectionLine(lines, p.Item1));

        return reflectionLines.Select(x => x.Item1).ToList();
    }
    
    private bool IsReflectionLine(List<string> lines, int indexBeforeReflection)
    {
        var i = 0;
        var minIndex = 0;
        var maxIndex = lines.Count - 1;
        while (true)
        {
            var low = indexBeforeReflection - i;
            var high = indexBeforeReflection + 1 + i;

            if (lines[low] != lines[high]) return false;
            
            if(low == minIndex || high == maxIndex) break;

            i++;
        }

        return true;
    }

    private ReflectionResult FindReflectionAfterCleaningSmudge(Pattern pattern)
    {
        // problem states that a smudge-cleaning must create a DIFFERENT reflection, so get the original reflection first for comparison
        var originalReflection = FindReflection(pattern);
        
        // try cleaning the smudge at each location until we find a new reflection
        for (int r = 0; r < pattern.Lines.Count; r++)
        {
            for (int c = 0; c < pattern.Lines[0].Length; c++)
            {
                var candidatePattern = CleanSmudgeAt(pattern, r, c);
                var result = FindReflection(candidatePattern, ignoreResult: originalReflection);
                if (result != null && result != originalReflection)
                {
                    // found a reflection after cleaning the smudge at (r,c), return this result
                    return result;
                }
            }
        }

        throw new Exception("No reflection found after cleaning all potential smudges");
    }

    private Pattern CleanSmudgeAt(Pattern pattern, int row, int column)
    {
        var newLines = new List<string>();
        for (int i = 0; i < pattern.Lines.Count; i++)
        {
            if (i == row)
            {
                var newChar = pattern.Lines[i][column] == '#' ? '.' : '#';
                newLines.Add(pattern.Lines[i].ReplaceAt(column, newChar));
            }
            else
            {
                newLines.Add(pattern.Lines[i]);    
            }
        }

        return new Pattern(newLines);
    }

    private List<Pattern> ParsePatterns(string data)
    {
        var patterns = data
            .ToParagraphs()
            .Select(p => p.ToLines(true))
            .Select(lines => new Pattern(lines.ToList()))
            .ToList();
        return patterns;
    }

    public record Pattern(List<string> Lines);

    public record ReflectionResult(int AfterIndex, ReflectionLineDirection LineDirection)
    {
        public long ResultValue()
        {
            var multiplier = LineDirection == ReflectionLineDirection.Vertical ? 1 : 100;
            var linesBeforeReflectionLine = AfterIndex + 1;
            return multiplier * linesBeforeReflectionLine;
        }
    }
        
    public enum ReflectionLineDirection { Vertical, Horizontal }
        
}

