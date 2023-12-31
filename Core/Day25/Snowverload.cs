using Core.Shared.Modules;

namespace Core.Day25;

public class Snowverload : BaseDayModule
{
    public Snowverload(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    public override int Day => 25;
    public override string Title => "Snowverload";

    [Fact] [ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(54);

    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    public long ExecutePart1(string input)
    {
        // This is graph theory that's above my head, using somebody else's solution here to learn from...
        // Reference: https://github.com/rtrinh3/AdventOfCode/blob/master/Aoc2023/Day25.cs
        
        // Inspired by https://www.reddit.com/r/adventofcode/comments/18qbsxs/comment/keug4yl
        // to find the clusters by putting the nodes on a number line and averaging their positions with their neighbors'

        // Parse
        const StringSplitOptions TrimAndDiscard =
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        string[] lines = input.ReplaceLineEndings("\n").Split('\n', TrimAndDiscard);
        HashSet<(string, string)> edges = new();
        DefaultDict<string, HashSet<string>> neighbors = new();
        foreach (string line in lines)
        {
            string[] parts = line.Split(Separators, TrimAndDiscard);
            string head = parts[0];
            var tails = parts.Skip(1);
            foreach (string tail in tails)
            {
                edges.Add(MakeEdge(head, tail));
                neighbors[head].Add(tail);
                neighbors[tail].Add(head);
            }
        }

        List<string> nodes = neighbors.Keys.ToList();

        LABEL_START:
        // From a random node, find the furthest node A
        string? furthestA = null;
        {
            string start = nodes[Random.Shared.Next(nodes.Count)];
            HashSet<string> visited = new();
            Queue<string> queue = new();
            queue.Enqueue(start);
            while (queue.TryDequeue(out var node))
            {
                visited.Add(node);
                furthestA = node;
                foreach (string next in neighbors[node])
                {
                    if (!visited.Contains(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }
        }
        System.Diagnostics.Debug.Assert(furthestA != null);

        // From A, find the furthest node B
        string? furthestB = null;
        {
            HashSet<string> visited = new();
            Queue<string> queue = new();
            queue.Enqueue(furthestA);
            while (queue.TryDequeue(out var node))
            {
                visited.Add(node);
                furthestB = node;
                foreach (string next in neighbors[node])
                {
                    if (!visited.Contains(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }
        }
        System.Diagnostics.Debug.Assert(furthestB != null);
        System.Diagnostics.Debug.Assert(furthestA != furthestB);

        // Initialize the nodes on the imaginary line
        DefaultDict<string, double> nodePos = new(() => 0.5);
        nodePos[furthestA] = 0;
        nodePos[furthestB] = 1;

        // Pull the nodes towards nodes A and B until the three links are visible
        {
            HashSet<string> visited = new();
            Queue<string> queue = new();
            queue.Enqueue(furthestA);
            queue.Enqueue(furthestB);
            while (queue.TryDequeue(out var node))
            {
                visited.Add(node);

                if (node != furthestA && node != furthestB)
                {
                    nodePos[node] = neighbors[node].Average(next => nodePos[next]);
                }

                foreach (string next in neighbors[node])
                {
                    if (!visited.Contains(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }
        }
        List<(string, string)> edgesStraddlingMiddle = edges.Where(e =>
                (nodePos[e.Item1] < 0.5 && 0.5 < nodePos[e.Item2]) ||
                (nodePos[e.Item2] < 0.5 && 0.5 < nodePos[e.Item1]))
            .ToList();
        // If I failed to make a clean cut, try again with another node
        if (edgesStraddlingMiddle.Count != 3)
        {
            goto LABEL_START;
        }

        // Count the clusters
        long leftCluster = 0;
        long rightCluster = 0;
        foreach (var node in nodes)
        {
            if (nodePos[node] < 0.5)
            {
                leftCluster++;
            }

            if (nodePos[node] > 0.5)
            {
                rightCluster++;
            }

            if (nodePos[node] == 0.5)
            {
                throw new Exception("How is this still in the middle");
            }
        }

        var solution = leftCluster * rightCluster;
        WriteLine($"Solution: {solution}");
        return solution;
    }

    private static (string, string) MakeEdge(string a, string b)
    {
        return (a.CompareTo(b) <= 0) ? (a, b) : (b, a);
    }

    private static readonly char[] Separators = { ':', ' ' };
}