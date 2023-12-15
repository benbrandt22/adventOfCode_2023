using System.Text;
using System.Text.RegularExpressions;
using Core.Shared.Extensions;
using Core.Shared.Modules;

namespace Core.Day15;

public class LensLibrary : BaseDayModule
{
    public LensLibrary(ITestOutputHelper outputHelper) : base(outputHelper) { }
    
    public override int Day => 15;
    public override string Title => "Lens Library";

    [Fact] public void Hash_Sample() => Hash("HASH").Should().Be(52);
    
    [Fact][ShowDebug] public void Part1_Sample() => ExecutePart1(GetData(InputType.Sample)).Should().Be(1320);
    [Fact] public void Part1() => ExecutePart1(GetData(InputType.Input));

    [Fact][ShowDebug] public void Part2_Sample() => ExecutePart2(GetData(InputType.Sample)).Should().Be(145);
    [Fact] public void Part2() => ExecutePart2(GetData(InputType.Input));

    public long ExecutePart1(string data)
    {
        var stringsToHash = data.Trim().Split(",");
        WriteLine($"Part 1 - Loaded {stringsToHash.Length} strings to hash");

        var totalOfHashedStrings = stringsToHash.Sum(Hash);
        WriteLine($"Total of hashed strings: {totalOfHashedStrings}");
        return totalOfHashedStrings;
    }
    
    public long ExecutePart2(string data)
    {
        var steps = data.Trim().Split(",").Select(x => new Step(x)).ToList();
        WriteLine($"Part 2 - Loaded {steps.Count} steps to perform");
        
        var lightBoxes = Enumerable.Range(0, 256).Select(i => new LightBox(i)).ToList();
        
        foreach (var step in steps)
        {
            Debug();
            Debug(step.ToString());
            
            switch (step.Operation)
            {
                case StepOperation.InsertLens:
                    lightBoxes[step.LightBoxId].InsertLens(new Lens(step.LensLabel, step.FocalLength!.Value));
                    break;
                case StepOperation.RemoveLens:
                    lightBoxes[step.LightBoxId].RemoveLens(step.LensLabel);
                    break;
                default:
                    throw new Exception($"Unknown operation {step.Operation}");
            }

            Debug(lightBoxes.Where(b => b.Lenses.Any()).Select(b => b.ToString())!.JoinWith("\r\n"));
        }

        var totalFocusingPower = lightBoxes.Sum(b => b.GetFocusingPower());
        WriteLine($"Total Focusing Power: {totalFocusingPower}");
        return totalFocusingPower;
    }

    public static int Hash(string input)
    {
        var currentValue = 0;
        foreach (var c in input)
        {
            currentValue += (int)c; // increase by ASCII value
            currentValue *= 17; // multiply current value by 17
            currentValue %= 256; // set current value to remainder of dividing by 256
        }

        return currentValue;
    }

    public class Step
    {
        private static readonly Regex ParseRegex = new Regex(@"^(?<LensLabel>[a-zA-Z]+)(?<Operation>[=-])(?<FocalLength>[0-9]?)$", RegexOptions.Compiled);
        public Step(string input)
        {
            Input = input;
            var match = ParseRegex.Match(input);
            LensLabel = match.Groups["LensLabel"].Value;
            LightBoxId = Hash(LensLabel);
            Operation = match.Groups["Operation"].Value switch
            {
                "-" => StepOperation.RemoveLens,
                "=" => StepOperation.InsertLens,
                _ => throw new Exception($"Unknown operation {match.Groups["Operation"].Value}")
            };
            FocalLength = int.TryParse(match.Groups["FocalLength"].Value, out var focalLength) ? focalLength : null;
        }
        public string Input { get; }
        public int LightBoxId { get; }
        public string LensLabel { get; }
        public StepOperation Operation { get; }
        public int? FocalLength { get; }
        
        public override string ToString() => $"Step \"{Input}\": Box {LightBoxId}, {Operation} {LensLabel} {FocalLength}";
    }
    
    public enum StepOperation
    {
        RemoveLens, // dash -
        InsertLens // equals =
    }

    public class LightBox
    {
        public int Id { get; }
        public List<Lens> Lenses { get; } = new();

        public LightBox(int id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"Box {Id}: {Lenses.Select(l => l.ToString()).JoinWith(" ")}";
        }

        public void InsertLens(Lens lens)
        {
            var existingLensIndex = Lenses.FindIndex(l => l.Label == lens.Label);
            if (existingLensIndex > -1)
            {
                Lenses.RemoveAt(existingLensIndex);
                Lenses.Insert(existingLensIndex, lens);
            }
            else
            {
                Lenses.Add(lens);
            }
        }
        
        public void RemoveLens(string label)
        {
            Lenses.RemoveAll(l => l.Label == label);
        }

        public int GetFocusingPower()
        {
            var focusingPower = Lenses
                .Select((lens, lensIndex) => (lens, lensIndex))
                .Sum(x =>
                {
                    var boxNum = (this.Id + 1);
                    var lensSlotNum = (x.lensIndex + 1);
                    var focusingPower = (boxNum * lensSlotNum * x.lens.FocalLength);
                    return focusingPower;
                });
            return focusingPower;
        }
    }

    public record Lens(string Label, int FocalLength)
    {
        public override string ToString() => $"[{Label} {FocalLength}]";
    }

}