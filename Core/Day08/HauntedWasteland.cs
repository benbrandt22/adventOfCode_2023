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

    [Fact(Skip = "Not yet implemented")] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public int ExecutePart1(string data)
    {
        var desertMap = ParseDesertMap(data);
        WriteLine($"Part 1 - Loaded Desert Map: Directions={desertMap.Directions.Count}, Nodes={desertMap.Nodes.Count}");

        var start = "AAA";
        var end = "ZZZ";
        var steps = desertMap.GetStepsForJourney(start, end);
        WriteLine($"Steps from {start} to {end}: {steps}");
        return steps;
    }
    
    public int ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
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
        
        var nodeRegEx = new Regex(@"(?<id>[A-Z]+) = \((?<left>[A-Z]+), (?<right>[A-Z]+)\)");

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

        public int GetStepsForJourney(string startNodeId, string endNodeId)
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
                    var nextNodeId = currentNode.Go(direction);
                    stepCount++;
                    if(nextNodeId == endNodeId) return stepCount;
                    currentNodeId = nextNodeId;
                }
            }
        }
    }
    
    public enum Direction { Left, Right }

    public record Node(string Id, string Left, string Right)
    {
        public string Go(Direction direction) => direction switch
        {
            Direction.Left => Left,
            Direction.Right => Right,
            _ => throw new Exception($"Unknown direction: {direction}")
        };
    }

}

