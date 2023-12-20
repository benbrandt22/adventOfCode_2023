using System.Diagnostics;
using Core.Shared;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day20;

public class PulsePropagation : BaseDayModule
{
    public PulsePropagation(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 20;
    public override string Title => "Pulse Propagation";

    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(32000000);
    [Fact][ShowDebug] public void Part1_Sample2() => ExecutePart1(GetData("sample2")).Should().Be(11687500);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact(Skip = "Not implemented")] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var commSystem = ParseCommSystem(data);
        WriteLine($"Part 1 - Loaded Comm System with {commSystem.Modules.Count} modules");

        long lowCounts = 0;
        long highCounts = 0;
        for (int i = 0; i < 1000; i++)
        {
            var thisPressSignalCounts = commSystem.GetSignalCountsForButtonPress();
            lowCounts += thisPressSignalCounts[Signal.Low];
            highCounts += thisPressSignalCounts[Signal.High];
        }
        
        WriteLine($"Low Signals:  {lowCounts}");
        WriteLine($"High Signals: {highCounts}");
        
        var solution = lowCounts * highCounts;
        WriteLine($"Solution: {solution}");
        return solution;
    }
    
    public long ExecutePart2(string data)
    {
        var commSystem = ParseCommSystem(data);
        WriteLine($"Part 2 - Loaded Comm System with {commSystem.Modules.Count} modules");

        throw new NotImplementedException();
        
        // TODO: find the cycles of the modules feeding into the last module before rx
        // TODO: use those cycles to get an LCM to figure out the solution
    }

    public CommSystem ParseCommSystem(string data)
    {
        var commSystem = new CommSystem();
        
        var config = data.ToLines(true).Select(line =>
        {
            var parts = line.Split(" -> ");
            var moduleTypeAndName = parts[0];
            var nextModuleNames = parts[1].Split(", ").ToList();
            var moduleType = moduleTypeAndName[0] switch
            {
                'b' => ModuleType.Broadcaster,
                '%' => ModuleType.FlipFlop,
                '&' => ModuleType.Conjunction,
                _ => throw new Exception($"Unknown module type: {moduleTypeAndName[0]}")
            };
            var moduleName = moduleType == ModuleType.Broadcaster ? moduleTypeAndName : moduleTypeAndName[1..];
            return new { moduleType, moduleName, nextModuleNames };
        }).ToList();
        
        commSystem.Modules.Add("button", new ButtonModule());
        
        var broadcaster = config.Single(c => c.moduleType == ModuleType.Broadcaster);
        commSystem.Modules.Add(broadcaster.moduleName, new BroadcasterModule(broadcaster.moduleName, broadcaster.nextModuleNames));
        
        var flipFlops = config.Where(c => c.moduleType == ModuleType.FlipFlop).ToList();
        flipFlops.ForEach(flipFlop => commSystem.Modules.Add(flipFlop.moduleName, new FlipFlopModule(flipFlop.moduleName, flipFlop.nextModuleNames)));
        
        var conjunctions = config.Where(c => c.moduleType == ModuleType.Conjunction).ToList();
        foreach (var conjunctionConfig in conjunctions)
        {
            var inputsToThisConjunction = config
                .Where(c => c.nextModuleNames.Contains(conjunctionConfig.moduleName))
                .Select(c => c.moduleName)
                .ToList();
            commSystem.Modules.Add(conjunctionConfig.moduleName, new ConjunctionModule(conjunctionConfig.moduleName, inputsToThisConjunction, conjunctionConfig.nextModuleNames));
        }
        
        var destinationNamesNotDefined = config.SelectMany(c => c.nextModuleNames).Distinct().Except(commSystem.Modules.Keys).ToList();
        destinationNamesNotDefined.ForEach(destinationName => commSystem.Modules.Add(destinationName, new EndModule(destinationName)));

        return commSystem;
    }

    public class CommSystem
    {
        public Dictionary<string, IModule> Modules { get; set; } = new();
        
        public Dictionary<Signal, int> GetSignalCountsForButtonPress()
        {
            Queue<(string InputSourceName, Signal Input, IModule Module)> _toProcess = new();
            
            _toProcess.Enqueue(("?", Signal.Low, Modules["button"]));
            
            Dictionary<Signal, int> signalCounts = new() { { Signal.Low, 0 }, { Signal.High, 0 } };

            while (_toProcess.Any())
            {
                var current = _toProcess.Dequeue();
                var output = current.Module.GetOutput(current.Input, current.InputSourceName);
                if (output != null)
                {
                    // count each output signal to each next module
                    foreach (var moduleNextModuleName in current.Module.NextModuleNames)
                    {
                        signalCounts[output.Value]++;
                        //System.Diagnostics.Debug.WriteLine($"{current.Module.Name} -{output.Value}-> {moduleNextModuleName}");
                        _toProcess.Enqueue((current.Module.Name, output.Value, Modules[moduleNextModuleName]));
                    }
                }
            }

            return signalCounts;
        }
    }
    
    public enum Signal { Low, High }
    
    public enum ModuleType { Button, Broadcaster, FlipFlop, Conjunction }
    
    public interface IModule
    {
        public string Name { get; }
        public List<string> NextModuleNames { get; }
        public Signal? GetOutput(Signal inputSignal, string inputSourceName);
    }

    public class ButtonModule : IModule
    {
        public string Name => "button";
        public List<string> NextModuleNames => new() { "broadcaster" };
        public Signal? GetOutput(Signal inputSignal, string inputSourceName) => Signal.Low; // button always send low
    }
    
    public class BroadcasterModule(string name, List<string> nextModuleNames) : IModule
    {
        public string Name { get; } = name;
        public List<string> NextModuleNames { get; } = nextModuleNames;
        public Signal? GetOutput(Signal inputSignal, string inputSourceName) => inputSignal;
    }
    
    public class FlipFlopModule(string name, List<string> nextModuleNames) : IModule
    {
        public string Name { get; } = name;
        public List<string> NextModuleNames { get; } = nextModuleNames;
        private bool _isOn = false;
        public Signal? GetOutput(Signal inputSignal, string inputSourceName)
        {
            // If a flip-flop module receives a high pulse, it is ignored and nothing happens
            if (inputSignal == Signal.High) return null;
            // However, if a flip-flop module receives a low pulse, it flips between on and off
            _isOn = !_isOn;
            // If it was off, it turns on and sends a high pulse. If it was on, it turns off and sends a low pulse
            return _isOn ? Signal.High : Signal.Low;
        }
    }
    
    public class ConjunctionModule : IModule
    {
        public string Name { get; }
        public List<string> NextModuleNames { get; }
        
        private Dictionary<string,Signal> _recentPulses;

        public ConjunctionModule(string name, List<string> inputModuleNames, List<string> nextModuleNames)
        {
            Name = name;
            NextModuleNames = nextModuleNames;
            // initially default to remembering a low pulse for each input
            _recentPulses = inputModuleNames.ToDictionary(inputModuleName => inputModuleName, inputModuleName => Signal.Low);
        }
        
        public Signal? GetOutput(Signal inputSignal, string inputSourceName)
        {
            // When a pulse is received, the conjunction module first updates its memory for that input.
            _recentPulses[inputSourceName] = inputSignal;
            // Then, if it remembers high pulses for all inputs, it sends a low pulse; otherwise, it sends a high pulse.
            var allInputsAreHigh = _recentPulses.Values.All(signal => signal == Signal.High);
            return allInputsAreHigh ? Signal.Low : Signal.High;
        }
    }
    
    public class EndModule(string name) : IModule
    {
        public string Name { get; } = name;
        public List<string> NextModuleNames => new ();
        public Signal? GetOutput(Signal inputSignal, string inputSourceName) => null;
    }
}



