using Core.Shared;

namespace Tests;

public class RangeTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void Range_throwsException_whenMinIsBeforeMax()
    {
        Action action = () => new Range<int>(2, 1);
        action.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Range_doesNotThrowException_whenMinIsEqualToMax()
    {
        Action action = () => new Range<int>(2, 2);
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(10, 15, 20, 25, false)]
    [InlineData(15, 20, 20, 25, true)]
    [InlineData(15, 22, 20, 25, true)]
    [InlineData(15, 25, 20, 25, true)]
    [InlineData(20, 25, 20, 25, true)]
    [InlineData(22, 25, 20, 25, true)]
    [InlineData(22, 30, 20, 25, true)]
    [InlineData(25, 30, 20, 25, true)]
    [InlineData(26, 30, 20, 25, false)]
    [InlineData(30, 35, 20, 25, false)]
    public void Range_intersectsWith_worksAsExpected(int minA, int maxA, int minB, int maxB, bool expected)
    {
        outputHelper.WriteLine($"[{minA}-{maxA}] [{minB}-{maxB}] IntersectsWith expected: {expected}");
        var rangeA = new Range<int>(minA, maxA);
        var rangeB = new Range<int>(minB, maxB);
        rangeA.IntersectsWith(rangeB).Should().Be(expected);
        rangeB.IntersectsWith(rangeA).Should().Be(expected);
    }
    
    [Theory]
    [InlineData(15, 20, 20, 25, 20,20)]
    [InlineData(15, 22, 20, 25, 20,22)]
    [InlineData(15, 25, 20, 25, 20,25)]
    [InlineData(20, 25, 20, 25, 20,25)]
    [InlineData(22, 25, 20, 25, 22,25)]
    [InlineData(22, 30, 20, 25, 22,25)]
    [InlineData(25, 30, 20, 25, 25,25)]
    public void Range_Intersection_returnsExpectedRange(int minA, int maxA, int minB, int maxB, int expectedMin, int expectedMax)
    {
        outputHelper.WriteLine($"[{minA}-{maxA}] [{minB}-{maxB}] Intersect expected: [{expectedMin}-{expectedMax}]");
        var rangeA = new Range<int>(minA, maxA);
        var rangeB = new Range<int>(minB, maxB);
        var expectedRange = new Range<int>(expectedMin, expectedMax);
        rangeA.Intersection(rangeB).Should().BeEquivalentTo(expectedRange);
        rangeB.Intersection(rangeA).Should().BeEquivalentTo(expectedRange);
    }
    
    [Theory]
    [InlineData(10, 15, 20, 25)]
    [InlineData(30, 35, 20, 25)]
    public void Range_Intersection_returnsNull_whenRangesDoNotIntersect(int minA, int maxA, int minB, int maxB)
    {
        outputHelper.WriteLine($"Non-intersecting ranges: [{minA}-{maxA}] [{minB}-{maxB}]");
        var rangeA = new Range<int>(minA, maxA);
        var rangeB = new Range<int>(minB, maxB);
        rangeA.Intersection(rangeB).Should().BeNull();
        rangeB.Intersection(rangeA).Should().BeNull();
    }
    
    [Fact]
    public void Range_Subtract_returnsOriginalRange_whenRangesDoNotIntersect()
    {
        var rangeA = new Range<int>(10, 15);
        var rangeB = new Range<int>(20, 25);
        rangeA.Subtract(rangeB).Should().BeEquivalentTo(new List<Range<int>> {rangeA});
        rangeB.Subtract(rangeA).Should().BeEquivalentTo(new List<Range<int>> {rangeB});
    }

    [Fact]
    public void Range_Subtract_returnsEmpty_whenRangesAreEqual()
    {
        var rangeA = new Range<int>(10, 15);
        var rangeB = new Range<int>(10, 15);
        rangeA.Subtract(rangeB).Should().BeEmpty();
        rangeB.Subtract(rangeA).Should().BeEmpty();
    }
    
    [Theory]
    [ClassData(typeof(RangeSubtractTestData))]
    public void Range_Subtract_returnsExpectedRanges_whenRangesIntersect(Range<int> rangeA, Range<int> rangeB, List<Range<int>> expected)
    {
        outputHelper.WriteLine($"[{rangeA.Min} to {rangeA.Max}] Subtract [{rangeB.Min} to {rangeB.Max}]");
        expected.ForEach(x => outputHelper.WriteLine($"Expected: [{x.Min}-{x.Max}]"));
        rangeA.Subtract(rangeB).Should().BeEquivalentTo(expected);
    }

    public class RangeSubtractTestData : TheoryData<Range<int>, Range<int>, List<Range<int>>>
    {
        public RangeSubtractTestData()
        {
            Add(new(0, 10), new(10, 20), new List<Range<int>> { new(0, 10) });
            Add(new(0, 10), new(-5, 5), new List<Range<int>> { new(5, 10) });
            Add(new(0, 10), new(0, 5), new List<Range<int>> { new(5, 10) });
            Add(new(0, 10), new(7, 10), new List<Range<int>> { new(0, 7) });
            Add(new(0, 10), new(7, 15), new List<Range<int>> { new(0, 7) });
            Add(new(0, 10), new(2, 7), new List<Range<int>> { new(0, 2), new(7, 10) });
        }
    }
    
}
