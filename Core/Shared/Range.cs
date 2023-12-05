namespace Core.Shared;

public class Range<T> where T:IComparable<T>
{
    public T Min { get; }
    public T Max { get; }
    public Range(T min, T max)
    {
        if (max.CompareTo(min) < 0) { throw new ArgumentException("min must not come after max"); }
        Min = min;
        Max = max;
    }

    public bool IntersectsWith(Range<T> other)
    {
        return Min.CompareTo(other.Max) <= 0 && Max.CompareTo(other.Min) >= 0;
    }
    
    public Range<T>? Intersection(Range<T> other)
    {
        if (!this.IntersectsWith(other))
        {
            return null;
        }
        var min = Min.CompareTo(other.Min) > 0 ? Min : other.Min;
        var max = Max.CompareTo(other.Max) < 0 ? Max : other.Max;
        return new Range<T>(min, max);
    }
    
    public List<Range<T>> Subtract(Range<T> other)
    {
        if (!this.IntersectsWith(other))
        {
            return new List<Range<T>> {this};
        }
        var result = new List<Range<T>>();
        if (Min.CompareTo(other.Min) < 0)
        {
            result.Add(new Range<T>(Min, other.Min));
        }
        if (Max.CompareTo(other.Max) > 0)
        {
            result.Add(new Range<T>(other.Max, Max));
        }
        return result;
    }
    
}