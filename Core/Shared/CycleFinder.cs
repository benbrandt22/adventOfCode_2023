namespace Core.Shared;

public static class CycleFinder
{
    /// <summary>
    /// Finds the repeating cycle in the input sequence, if any. Assumes logic is deterministic and produces a repeating cycle.
    /// </summary>
    public static CycleAnalysis<T> FindCycle<T>(IEnumerable<T> inputCycle, long maxIterations = long.MaxValue)
        where T : IEquatable<T>
    {
        return FindCycle(inputCycle, (a, b) => a.Equals(b), maxIterations);
    }

    /// <summary>
    /// Finds the repeating cycle in the input sequence, if any. Assumes logic is deterministic and produces a repeating cycle.
    /// </summary>
    public static CycleAnalysis<T> FindCycle<T>(IEnumerable<T> inputCycle,
        Func<T, T, bool> equalityPredicate,
        long maxIterations = long.MaxValue)
    {
        var valuesSeen = new List<T>();

        using var inputEnumerator = inputCycle.GetEnumerator();
        
        for (int i = 0; i < maxIterations; i++)
        {
            inputEnumerator.MoveNext();
            var current = inputEnumerator.Current;

            var seenAtIndex = valuesSeen.FindIndex(x => equalityPredicate(x, current));
            
            if (seenAtIndex > -1)
            {
                var cycleStartIndex = seenAtIndex;
                var cycleLength = (i - seenAtIndex);
                return new CycleAnalysis<T>(cycleStartIndex, cycleLength, valuesSeen);
            }

            valuesSeen.Add(current);
        }

        throw new InvalidOperationException(
            $"No cycle found in input sequence after checking {maxIterations} iterations.");
    }

    public class CycleAnalysis<T>
    {
        public int CycleStartIndex { get; }
        public int CycleLength { get; }
        public List<T> ValuesSeen { get; }

        public CycleAnalysis(int cycleStartIndex, int cycleLength, List<T> valuesSeen)
        {
            CycleStartIndex = cycleStartIndex;
            CycleLength = cycleLength;
            ValuesSeen = valuesSeen;
        }
        
        // TODO: add method for extrapolating the value at a given index (see Day 14)
    }
}