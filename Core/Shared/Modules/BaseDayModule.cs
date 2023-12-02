namespace Core.Shared.Modules;

public abstract class BaseDayModule : IDayModule
{
    private readonly ITestOutputHelper _outputHelper;

    public BaseDayModule(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        
        // the test class gets instantiated once per test, so we can output the header here to make it display at the start of each unit test
        OutputHeader();
    }

    private void OutputHeader()
    {
        WriteHorizontalRule();
        WriteLine($"Advent of Code 2023 - Day {Day}: {Title}");
        WriteHorizontalRule();
    }

    public abstract int Day { get; }
    public abstract string Title { get; }

    /// <summary>
    /// When set to true, calls to Debug() will be written to the output
    /// </summary>
    public static bool DebugEnabled { get; set; }
    
    internal void WriteLine(string line = "") => _outputHelper.WriteLine(line);

    internal void Debug(string line = "")
    {
        if (DebugEnabled)
        {
            WriteLine(line);
        }
    }

    internal void WriteHorizontalRule(int length = 80) => WriteLine(new string('-', length));
}