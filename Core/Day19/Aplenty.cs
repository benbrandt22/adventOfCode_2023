using System.Text.RegularExpressions;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day19;

public class Aplenty : BaseDayModule
{
    public Aplenty(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 19;
    public override string Title => "Aplenty";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(19114);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not yet implemented")][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(-1);
    [Fact(Skip = "Not yet implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var (workflows,parts) = ParseInput(data);
        
        WriteLine($"Part 1 - Loaded {workflows.Count} workflows and {parts.Count} parts");

        var finalDestinations = parts.Select(part => (Part : part, Status : GetFinalDestination(workflows, part))).ToList();
        
        var acceptedParts = finalDestinations
            .Where(x => x.Status == "A")
            .Select(x => x.Part)
            .ToList();
        
        WriteLine($"Accepted Parts: {acceptedParts.Count}");
        var totalValue = acceptedParts.Sum(x => x.TotalValue);
        
        WriteLine($"Total value of accepted parts: {totalValue}");
        return totalValue;
    }

    public long ExecutePart2(string data)
    {
        WriteLine($"Part 2 - Loaded Data");

        var solution = 0;
        WriteLine($"Solution: {solution}");
        return solution;
    }
    
    private (Dictionary<string,Workflow> Workflows, List<Part>) ParseInput(string data)
    {
        var sections = data.Trim().ToParagraphs();
        var workflowSection = sections[0];
        var partSection = sections[1];

        var workflows = new Dictionary<string, Workflow>();
        // example: px{a<2006:qkq,m>2090:A,rfg}
        var workflowRegEx1 = new Regex(@"^(?<id>\w+){(?<steps>.*)}$");
        var predicateRegEx = new Regex(@"^(?<field>\w+)(?<operator>[<>]+)(?<value>\d+)$");
        
        foreach (var line in workflowSection.ToLines(true))
        {
            var wfMatch = workflowRegEx1.Match(line);
            var workflowId = wfMatch.Groups["id"].Value;
            var workflowSteps = wfMatch.Groups["steps"].Value.Split(',')
                .Select(instruction =>
                {
                    WorkflowStepPredicate predicate = WorkflowStepPredicate.AlwaysValid;
                    if (instruction.Contains(':'))
                    {
                        var predicateMatch = predicateRegEx.Match(instruction.Split(":").First());
                        var field = predicateMatch.Groups["field"].Value;
                        var op = predicateMatch.Groups["operator"].Value;
                        var value = long.Parse(predicateMatch.Groups["value"].Value);
                        predicate = new WorkflowStepPredicate(field, op, value);
                    }
                    
                    string destination = instruction.Split(":").Last();
                    
                    return new WorkflowStep(predicate, destination);
                }).ToList();
            var workflow = new Workflow(workflowId, workflowSteps);
            workflows.Add(workflowId, workflow);
        }
        
        // example: {x=787,m=2655,a=1222,s=2876}
        var partRegEx = new Regex(@"^{x=(?<x>\d+),m=(?<m>\d+),a=(?<a>\d+),s=(?<s>\d+)}$");
        var parts = partSection.Trim().ToLines()
            .Select(line =>
            {
                var m = partRegEx.Match(line);
                return new Part(
                    long.Parse(m.Groups["x"].Value),
                    long.Parse(m.Groups["m"].Value),
                    long.Parse(m.Groups["a"].Value),
                    long.Parse(m.Groups["s"].Value)
                );
            })
            .ToList();
        
        return (workflows, parts);
    }
    
    public string GetFinalDestination(Dictionary<string, Workflow> workflows, Part part, string startingWorkflowId = "in")
    {
        var workflow = workflows[startingWorkflowId];
        while (true)
        {
            var workflowId = workflow.EvaluateAndGetDestination(part);
            if (!workflows.ContainsKey(workflowId))
            {
                return workflowId;
            }
            workflow = workflows[workflowId];
        }
    }

    public class Workflow
    {
        public Workflow(string id, List<WorkflowStep> steps)
        {
            Id = id;
            Steps = steps;
        }

        public string Id { get; }
        public List<WorkflowStep> Steps { get; }

        public string EvaluateAndGetDestination(Part part)
        {
            foreach (var step in Steps)
            {
                var destination = step.EvaluateAndGetDestination(part);
                if (destination != null) return destination;
            }

            throw new Exception("No valid destination found");
        }
    }

    public class WorkflowStep
    {
        public WorkflowStepPredicate Predicate { get; }
        public string Destination { get; }

        public WorkflowStep(WorkflowStepPredicate predicate, string destination)
        {
            Predicate = predicate;
            Destination = destination;
        }
        
        /// <summary>
        /// Returns the destination workflow ID if the predicate evaluates to true, otherwise null
        /// </summary>
        public string? EvaluateAndGetDestination(Part part)
        {
            return Predicate.Evaluate(part) ? Destination : null;
        }
    }

    public class WorkflowStepPredicate
    {
        public string Field { get; }
        public string Op { get; }
        public long Value { get; }
        private readonly bool _shouldEvaluate;

        public WorkflowStepPredicate(string field, string op, long value) : this(true, field, op, value)
        {
        }

        private WorkflowStepPredicate(bool shouldEvaluate, string field = "", string op = "", long value = 0)
        {
            _shouldEvaluate = shouldEvaluate;
            Field = field;
            Op = op;
            Value = value;
        }
        
        public static WorkflowStepPredicate AlwaysValid = new WorkflowStepPredicate(false);

        public bool Evaluate(Part part)
        {
            if (!_shouldEvaluate) return true; // part always passes
            
            var partValue = Field switch
            {
                "x" => part.X,
                "m" => part.M,
                "a" => part.A,
                "s" => part.S,
                _ => throw new Exception($"Unknown field {Field}")
            };
            
            var isValid = Op switch
            {
                "<" => partValue < Value,
                ">" => partValue > Value,
                _ => throw new Exception($"Unknown operator {Op}")
            };
            return isValid;
        }
    }

    public record Part(long X, long M, long A, long S)
    {
        public long TotalValue => X + M + A + S;
    }
}