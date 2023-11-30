using Core.Shared;
using Core.Shared.Modules;

namespace Core.Day01;

public class ToBeDetermined : BaseDayModule
{
    public override int Day => 1;
    public override void Execute()
    {
        Process("Day01/day1-sample.txt");
        Process("Day01/day1-input.txt");
    }

    private void Process(string filename)
    {
        var text = TextFileLoader.LoadText(filename);
        
        WriteLine($"Loaded {filename} with {text.Length} characters.");
    }
}