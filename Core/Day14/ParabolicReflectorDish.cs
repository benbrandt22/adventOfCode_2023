using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day14;

public class ParabolicReflectorDish : BaseDayModule
{
    public ParabolicReflectorDish(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 14;
    public override string Title => "Parabolic Reflector Dish";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(136);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not yet implemented")] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var dish = ParseReflectorDish(data);
        WriteLine($"Part 1 - Loaded Reflector Dish with {dish.Rows} rows and {dish.Columns} columns");

        throw new NotImplementedException();
        
        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }
    
    public long ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }
    
    private ReflectorDish ParseReflectorDish(string data)
    {
        var lines = data.ToLines(true);
        return new ReflectorDish(lines);
    }

    public class ReflectorDish
    {
        public ReflectorDish(List<string> lines) => Lines = lines;

        public List<string> Lines { get; }

        public int Rows => Lines.Count;
        public int Columns => Lines[0].Length;

    }
}

