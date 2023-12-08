using System.Text.RegularExpressions;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day08;

public class HauntedWasteland : BaseDayModule
{
    public HauntedWasteland(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 8;
    public override string Title => "Haunted Wasteland";

    [Fact] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(2);
    [Fact] public void Part1_Sample2() => ExecutePart1(GetData("sample2")).Should().Be(6);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact] public void Part2_Sample() => ExecutePart2(GetData("sample_part2")).Should().Be(6);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public int ExecutePart1(string data)
    {
        var desertMap = ParseDesertMap(data);
        WriteLine($"Part 1 - Loaded Desert Map: Directions={desertMap.Directions.Count}, Nodes={desertMap.Nodes.Count}");

        var start = "AAA";
        var end = "ZZZ";
        var steps = desertMap.GetStepsForWalkingJourney(start, end);
        WriteLine($"Steps from {start} to {end}: {steps}");
        return steps;
    }
    
    public long ExecutePart2(string data)
    {
        var desertMap = ParseDesertMap(data);
        WriteLine($"Part 2 - Loaded Desert Map: Directions={desertMap.Directions.Count}, Nodes={desertMap.Nodes.Count}");

        var ghostSteps = desertMap.GetStepsForMultiGhostJourney();
        WriteLine($"Total steps for Multi-Ghost journey: {ghostSteps}");
        return ghostSteps;
    }

    private DesertMap ParseDesertMap(string data)
    {
        var directions = data.ToLines()[0].Trim()
            .Select(c => c switch
            {
                'L' => Direction.Left,
                'R' => Direction.Right,
                _ => throw new Exception($"Unknown direction: {c}")
            }).ToList();
        
        var nodeRegEx = new Regex(@"(?<id>[A-Z0-9]{3}) = \((?<left>[A-Z0-9]{3}), (?<right>[A-Z0-9]{3})\)");

        var nodes = nodeRegEx.Matches(data).MapEachTo<Node>().ToList();

        return new DesertMap(directions, nodes);
    }

    public class DesertMap
    {
        public DesertMap(List<Direction> directions, List<Node> nodes)
        {
            Directions = directions;
            Nodes = nodes;
        }
        
        public List<Direction> Directions { get; }
        public List<Node> Nodes { get; }

        public int GetStepsForWalkingJourney(string startNodeId, string endNodeId)
        {
            var nodeDictionary = Nodes.ToDictionary(n => n.Id, n => n);
            // traverse the nodes, following directions, and count how many steps to get to the end
            int stepCount = 0;
            var currentNodeId = startNodeId;
            while (true)
            {
                foreach (var direction in Directions)
                {
                    var currentNode = nodeDictionary[currentNodeId];
                    var nextNodeId = currentNode.GetNextId(direction);
                    stepCount++;
                    if(nextNodeId == endNodeId) return stepCount;
                    currentNodeId = nextNodeId;
                }
            }
        }

        public long GetStepsForMultiGhostJourney()
        {
            var nodeDictionary = Nodes.ToDictionary(n => n.Id, n => n);
            var startingGhostNodes = Nodes.Where(n => n.Id.EndsWith("A")).ToList();

            var ghostPaths = startingGhostNodes
                .Select(startNode => new
                    { StartNode = startNode, Path = NodesForSingleGhostJourney(nodeDictionary, startNode) })
                .Select(x =>
                {
                    var stepsToEnd = x.Path.TakeWhile(n => !n.Id.EndsWith("Z")).Count();
                    return new { x.StartNode, StepsToEnd = stepsToEnd };
                }).ToList();
            
            // each ghost path has a certain distance to reach an end point, and then repeats
            // find the least common multiple and you'll get the total distance where they all land on an end node together
            // (I can't take full credit for this realization. The AoC subreddit gave me a clue, and then you can see it in the part 2 example they laid out)
            
            var ghostPathLengths = ghostPaths.Select(x => (long)x.StepsToEnd).ToList();
            var totalSteps = LeastCommonMultiple(ghostPathLengths);

            return totalSteps;
        }

        private IEnumerable<Node> NodesForSingleGhostJourney(Dictionary<string,Node> nodes, Node startingNode)
        {
            var currentNode = startingNode;
            while (true)
            {
                foreach (var direction in Directions)
                {
                    yield return currentNode;
                    var nextNode = nodes[currentNode.GetNextId(direction)];
                    currentNode = nextNode;
                }
            }
        }
    }
    
    public enum Direction { Left, Right }

    public record Node(string Id, string Left, string Right)
    {
        public string GetNextId(Direction direction) => direction switch
        {
            Direction.Left => Left,
            Direction.Right => Right,
            _ => throw new Exception($"Unknown direction: {direction}")
        };
    }
    
    /// <summary>
    /// Least Common Multiple
    /// From: https://www.w3resource.com/csharp-exercises/math/csharp-math-exercise-20.php
    /// </summary>
    public static long LeastCommonMultiple(IEnumerable<long> numbers)
    {
        return numbers.Aggregate((S, val) => S * val / gcd(S, val));
    }
    
    static long gcd(long n1, long n2)
    {
        if (n2 == 0)
        {
            return n1;
        }
        else
        {
            return gcd(n2, n1 % n2);
        }
    }

}

