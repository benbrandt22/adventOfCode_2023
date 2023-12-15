using Core.Shared;

namespace Tests;

public class CycleFinderTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(new[] { 1, 2, 3, 1, 2, 3 }, 0, 3)] // cyclical values only
    [InlineData(new[] { 99, 100, 1, 2, 1, 2 }, 2, 2)] // some other values before leading into the cycle
    public void CycleFinder_identifies_cycle_as_expected(int[] inputCycle, int expectedCycleStartIndex,
        int expectedCycleLength)
    {
        outputHelper.WriteLine($"[{string.Join(",",inputCycle)}]");
        outputHelper.WriteLine($"Expected start index {expectedCycleStartIndex} & cycle length {expectedCycleLength}");
        
        var cycleAnalysis = CycleFinder.FindCycle(inputCycle);
        cycleAnalysis.CycleStartIndex.Should().Be(expectedCycleStartIndex);
        cycleAnalysis.CycleLength.Should().Be(expectedCycleLength);
    }

    [Fact]
    public void CycleFinder_throws_when_no_cycle_found_at_end_of_series()
    {
        var inputCycle = new[] { 1, 2, 3, 4, 5, 6 };
        Action act = () => CycleFinder.FindCycle(inputCycle);
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void CycleFinder_throws_when_no_cycle_found_after_checking_max_iterations()
    {
        var inputCycle = "abcdefghijklmnopqrstuvwxyz";
        var maxIterations = 20;
        Action act = () => CycleFinder.FindCycle(inputCycle, maxIterations);
        act.Should().Throw<InvalidOperationException>().Where(e => e.Message.Contains(maxIterations.ToString()));
    }
    
    [Fact]
    public void CycleFinder_uses_equality_predicate_to_identify_value_match()
    {
        var inputCycle = new List<Person>
        {
            new("Ben"), new("Beth"), new("Ben"), new("Beth")
        };
        var cycleAnalysis = CycleFinder.FindCycle(inputCycle, (p1, p2) => p1.Name.Equals(p2.Name));
        cycleAnalysis.CycleStartIndex.Should().Be(0);
        cycleAnalysis.CycleLength.Should().Be(2);
    }

    private class Person(string name)
    {
        public string Name { get; } = name;
    }
    
    [Theory]
    // Extrapolate to indexes past the end of the supplied data
    [InlineData(new[] { 1, 2, 3, 1, 2, 3 }, 8, 3)]
    [InlineData(new[] { 99, 100, 1, 2, 1, 2 }, 7, 2)]
    // Asking for an index within what has already been seen (no extrapolation needed)
    [InlineData(new[] { 1, 2, 3, 1, 2, 3 }, 4, 2)]
    [InlineData(new[] { 99, 100, 1, 2, 1, 2 }, 1, 100)]
    public void FindValueAt_extrapolates_value_as_expected(int[] inputCycle, int targetIndex, int expectedValue)
    {
        outputHelper.WriteLine($"[{string.Join(",",inputCycle)}]");
        outputHelper.WriteLine($"Expecting {expectedValue} at index {targetIndex}");
        
        var cycleAnalysis = CycleFinder.FindCycle(inputCycle);
        var extrapolatedValue = cycleAnalysis.FindValueAt(targetIndex);
        extrapolatedValue.Should().Be(expectedValue);
    }
}
