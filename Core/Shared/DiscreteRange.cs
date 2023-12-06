using System.Diagnostics;

namespace Core.Shared;

/// <summary>
/// Range class for discrete numbers. Treats the range as a set of numbers,
/// not a continuous range like date/time ranges or decimal/double ranges.
/// The main difference is the behavior of the subtraction method.
/// </summary>
[DebuggerDisplay("[{Min} to {Max}]")]
public class DiscreteRange
{
    public long Min { get; }
    public long Max { get; }
    public DiscreteRange(long min, long max)
    {
        if (max < min) { throw new ArgumentException("min must not come after max"); }
        Min = min;
        Max = max;
    }

    public bool IntersectsWith(DiscreteRange other)
    {
        return Min <= other.Max && Max >= other.Min;
    }
    
    public DiscreteRange? Intersection(DiscreteRange other)
    {
        if (!this.IntersectsWith(other))
        {
            return null;
        }
        var min = Min > other.Min ? Min : other.Min;
        var max = Max < other.Max ? Max : other.Max;
        return new DiscreteRange(min, max);
    }
    
    public List<DiscreteRange> Subtract(DiscreteRange other)
    {
        if (!this.IntersectsWith(other))
        {
            return new List<DiscreteRange> {this};
        }
        var result = new List<DiscreteRange>();
        if (Min < other.Min)
        {
            result.Add(new DiscreteRange(Min, other.Min - 1));
        }
        if (Max > other.Max)
        {
            result.Add(new DiscreteRange(other.Max + 1, Max));
        }
        return result;
    }
    
}